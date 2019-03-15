using System;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Npgsql;
using System.Data.Common;

namespace Titanoboa
{
    public static class LogXmlHelper
    {
        public static async Task CreateLog(string xmlFilename, User user = null)
        {
            using (var connection = await SqlHelper.GetConnection())
            using (var logCommand = SqlHelper.GetCommand(connection))
            {
                logCommand.CommandText = @"SELECT *, users.username AS username FROM logs 
                                           LEFT JOIN users ON users.id = logs.userid";
                
                if (user != null)
                {
                    logCommand.CommandText += @" WHERE users.id = @userid";
                    logCommand.Parameters.AddWithValue("@userid", user.Id);
                }

                await logCommand.PrepareAsync();

                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

                using (var xmlWriter = XmlWriter.Create(xmlFilename, settings))
                using (var reader = await logCommand.ExecuteReaderAsync())
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("log");

                    while (await reader.ReadAsync())
                    {
                        var logType = reader["logtype"].ToString();
                        switch (logType)
                        {
                            case "command":
                                xmlWriter.WriteStartElement("userCommand");
                                WriteRequiredValues(xmlWriter, reader);
                                WriteCommonValues(xmlWriter, reader);
                                xmlWriter.WriteEndElement();
                                break;
                            
                            case "quote":
                                xmlWriter.WriteStartElement("quoteServer");
                                WriteRequiredValues(xmlWriter, reader);


                                xmlWriter.WriteElementString("quoteServerTime", reader["quoteservertime"].ToString());

                                // NOTE: We store the returned username in the message column
                                xmlWriter.WriteElementString("username", reader["message"].ToString());

                                xmlWriter.WriteElementString("stockSymbol", reader["stocksymbol"].ToString());
                                xmlWriter.WriteElementString("price", reader["amount"].ToString());
                                xmlWriter.WriteElementString("cryptokey", reader["cryptokey"].ToString());

                                xmlWriter.WriteEndElement();
                                break;

                            case "transaction":
                                xmlWriter.WriteStartElement("accountTransaction");
                                WriteRequiredValues(xmlWriter, reader);

                                xmlWriter.WriteElementString("action", reader["message"].ToString());
                                xmlWriter.WriteElementString("username", reader["username"].ToString());
                                xmlWriter.WriteElementString("funds", reader["amount"].ToString());

                                xmlWriter.WriteEndElement();
                                break;

                            case "system":
                                xmlWriter.WriteStartElement("systemEvent");
                                WriteRequiredValues(xmlWriter, reader);
                                WriteCommonValues(xmlWriter, reader);
                                xmlWriter.WriteEndElement();
                                break;

                            case "error":
                                xmlWriter.WriteStartElement("errorEvent");
                                WriteRequiredValues(xmlWriter, reader);
                                WriteCommonValues(xmlWriter, reader);

                                xmlWriter.WriteElementString("errorMessage", reader["message"].ToString());

                                xmlWriter.WriteEndElement();
                                break;

                            case "debug":
                                xmlWriter.WriteStartElement("debugEvent");
                                WriteRequiredValues(xmlWriter, reader);
                                WriteCommonValues(xmlWriter, reader);

                                xmlWriter.WriteElementString("debugMessage", reader["message"].ToString());

                                xmlWriter.WriteEndElement();
                                break;
                        }
                    }
                    
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                }
            }

        }

        private static void WriteRequiredValues(XmlWriter xmlWriter, DbDataReader reader)
        {
            xmlWriter.WriteElementString("timestamp", UnixTimestamp((DateTime)reader["timestamp"]));
            xmlWriter.WriteElementString("server", reader["server"].ToString());
            xmlWriter.WriteElementString("transactionNum", reader["workid"].ToString());
        }

        private static void WriteCommonValues(XmlWriter xmlWriter, DbDataReader reader)
        {
            xmlWriter.WriteElementString("command", reader["command"].ToString());

            var username = reader["username"].ToString();
            var stockSymbol = reader["stocksymbol"].ToString();
            var filename = reader["filename"].ToString();
            var funds = reader["amount"].ToString();

            if (!string.IsNullOrEmpty(username)) xmlWriter.WriteElementString("username", username);
            if (!string.IsNullOrEmpty(stockSymbol)) xmlWriter.WriteElementString("stockSymbol", stockSymbol);
            if (!string.IsNullOrEmpty(filename)) xmlWriter.WriteElementString("filename", filename);
            if (!string.IsNullOrEmpty(funds)) xmlWriter.WriteElementString("funds", funds);
        }

        private static string UnixTimestamp(DateTime time)
        {
            return Math.Round((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000)).ToString();
        }
    }
}