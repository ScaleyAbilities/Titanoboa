using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Xml;
using Xsd.Logfile.cs;

namespace Titanoboa
{
    public static class XmlHelper
    {
        public static XmlWriter Create()
        {
            XmlWriter logWriter = XmlWriter.Create("./Logfiles/Logfile.xml");
            logWriter.WriteStartDocument();
            return logWriter.WriteStartElement("Log");
        }
    }
}