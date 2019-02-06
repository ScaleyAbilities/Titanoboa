using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Titanoboa
{
    static class RabbitHelper
    {
        private static IConnection rabbitConnection;
        private static IModel rabbitChannel;

        private static string rabbitHost = Environment.GetEnvironmentVariable("RABBIT_HOST") ?? "localhost";
        private static string rabbitCommandQueue = "commands";
        private static IBasicProperties rabbitProperties;

        static RabbitHelper() 
        {
            // Ensure Rabbit Queue is set up
            var factory = new ConnectionFactory() { 
                HostName = rabbitHost,
                UserName = "scaley",
                Password = "abilities"
            };

            // Try connecting to rabbit until it works
            var connected = false;
            while (!connected)
            {
                try
                {
                    rabbitConnection = factory.CreateConnection();
                    connected = true;
                }
                catch (BrokerUnreachableException ex)
                {
                    Console.Error.WriteLine("Unable to connect to Rabbit, retrying...");
                    Thread.Sleep(3000);
                }
            }
            
            rabbitChannel = rabbitConnection.CreateModel();

            rabbitChannel.QueueDeclare(
                queue: rabbitCommandQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // This makes Rabbit wait for an ACK before sending us the next message
            rabbitChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            rabbitProperties = rabbitChannel.CreateBasicProperties();
            rabbitProperties.Persistent = true;
        }

        public static void CreateConsumer(Action<JObject> messageCallback)
        {
            var consumer = new EventingBasicConsumer(rabbitChannel);
            consumer.Received += (model, ea) =>
            {
                JObject message = null;
                try 
                {
                    message = JObject.Parse(Encoding.UTF8.GetString(ea.Body));
                }
                catch (JsonReaderException ex)
                {
                    Console.Error.WriteLine($"Unable to parse Queue message into JSON: {ex.Message}");
                }
                
                if (message != null)
                    messageCallback(message);

                // We will always ack even if we can't parse it otherwise queue will hang
                rabbitChannel.BasicAck(ea.DeliveryTag, false);
            };

            // This will begin consuming messages asynchronously
            rabbitChannel.BasicConsume(
                queue: rabbitCommandQueue,
                autoAck: false,
                consumer: consumer
            );
        }
    }
}