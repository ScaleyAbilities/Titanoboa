using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public static string rabbitCommandQueue = $"commands";
        public static string rabbitLogQueue = "logs";
        private static string rabbitTriggerPending = "triggerPending";
        public static string rabbitTriggerCompleted = $"triggerCompleted";
        public static string rabbitInstanceQueue = "instance";
        public static string rabbitResponseQueue = "response";
        private static IBasicProperties rabbitProperties;

        static RabbitHelper()
        {
            // Ensure Rabbit Queue is set up
            var factory = new ConnectionFactory()
            {
                HostName = rabbitHost,
                UserName = "scaley",
                Password = "abilities",
                DispatchConsumersAsync = true,
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
                catch (BrokerUnreachableException)
                {
                    Console.Error.WriteLine("Unable to connect to Rabbit, retrying...");
                    Thread.Sleep(3000);
                }
            }

            rabbitChannel = rabbitConnection.CreateModel();

            // This makes Rabbit wait for an ACK before sending us the next message
            rabbitChannel.BasicQos(prefetchSize: 0, prefetchCount: 200, global: false);

            rabbitProperties = rabbitChannel.CreateBasicProperties();
            rabbitProperties.Persistent = true;
        }

        public static void CreateQueues()
        {
            rabbitCommandQueue += $".{Program.InstanceId}";
            rabbitTriggerCompleted += $".{Program.InstanceId}";

            rabbitChannel.QueueDeclare(
                queue: rabbitCommandQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            rabbitChannel.QueueDeclare(
                queue: rabbitLogQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            rabbitChannel.QueueDeclare(
                queue: rabbitTriggerPending,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            rabbitChannel.QueueDeclare(
                queue: rabbitTriggerCompleted,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            rabbitChannel.QueueDeclare(
                queue: rabbitResponseQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        public static string GetInstance()
        {
            rabbitChannel.QueueDeclare(
                queue: rabbitInstanceQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            BasicGetResult message = null;

            while (message == null)
            {
                message = rabbitChannel.BasicGet(rabbitInstanceQueue, true);
                Thread.Sleep(1000);
            }
            
            var instance = Encoding.UTF8.GetString(message.Body);
            var nextInstance = int.Parse(instance) + 1;

            rabbitChannel.BasicPublish(
                exchange: "",
                routingKey: rabbitInstanceQueue,
                basicProperties: rabbitProperties,
                body: Encoding.UTF8.GetBytes(nextInstance.ToString())
            );

            return instance;
        }

        public static void CreateConsumer(Func<JObject, Task> messageCallback, string queue, int priority = 0)
        {
            var consumer = new AsyncEventingBasicConsumer(rabbitChannel);
            consumer.Received += async (model, eventArgs) =>
            {
                JObject message = null;
                try
                {
                    message = JObject.Parse(Encoding.UTF8.GetString(eventArgs.Body));
                }
                catch (JsonReaderException ex)
                {
                    Console.Error.WriteLine($"Unable to parse Queue message into JSON: {ex.Message}");
                }

                if (message != null)
                    await messageCallback(message);

                // We will always ack even if we can't parse it otherwise queue will hang
                rabbitChannel.BasicAck(eventArgs.DeliveryTag, false);
            };

            // This will begin consuming messages asynchronously
            rabbitChannel.BasicConsume(
                queue: queue,
                autoAck: false,
                consumer: consumer,
                arguments: new Dictionary<string, object>() { { RabbitMQ.Client.Headers.XPriority, priority } }
            );
        }

        public static void PushTrigger(JObject properties)
        {
            // Add return queue to props
            properties.Add("queue", rabbitTriggerCompleted);            

            rabbitChannel.BasicPublish(
                exchange: "",
                routingKey: rabbitTriggerPending,
                basicProperties: rabbitProperties,
                body: Encoding.UTF8.GetBytes(properties.ToString(Formatting.None))
            );
        }

        public static void PushLogEntry(string logEntry)
        {
            rabbitChannel.BasicPublish(
                exchange: "",
                routingKey: rabbitLogQueue,
                basicProperties: rabbitProperties,
                body: Encoding.UTF8.GetBytes(logEntry)
            );
        }

        public static void PushResponse(JObject responseJson)
        {
            rabbitChannel.BasicPublish(
                exchange: "",
                routingKey: rabbitResponseQueue,
                basicProperties: rabbitProperties,
                body: Encoding.UTF8.GetBytes(responseJson.ToString(Formatting.None))
            );
        }
        
        public static void CloseRabbit()
        {
            rabbitChannel.Close();
            rabbitConnection.Close();
        }
    }
}
