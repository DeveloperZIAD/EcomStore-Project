using System;
using Microsoft.Data.SqlClient;
using System.Xml.Linq;
using System.IO;

namespace Data_layer.Conation
{
   
        public static class ConnectionManager
        {
            private static string _connectionString;

            static ConnectionManager()
            {
                string xmlPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "DbConfig.xml"
                );
              

                if (!File.Exists(xmlPath))
                    throw new FileNotFoundException("DbConfig.xml not found", xmlPath);

                XElement xml = XElement.Load(xmlPath);

                _connectionString = xml
                    .Element("ConnectionString")?
                    .Value;

                if (string.IsNullOrWhiteSpace(_connectionString))
                    throw new Exception("ConnectionString is empty or missing in XML");
            }

            public static SqlConnection GetConnection()
            {
                return new SqlConnection(_connectionString);
            }

            public static void TestConnection()
            {
                using var conn = GetConnection();
                conn.Open();
               
            }
        }
    
}
