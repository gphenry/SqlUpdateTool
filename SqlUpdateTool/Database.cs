using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using FirebirdSql.Data.FirebirdClient;

namespace SqlUpdateTool
{
    public static class Database
    {
        public static DataTable ExecuteDataReader(DbConnection connection, string sql)
        {
            DataTable dataTable = new DataTable();
            dataTable.Clear();

            try
            {
                var wasOpen = false;

                if (connection.State != ConnectionState.Open)
                {
                    wasOpen = true;
                    connection.Open();
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    using (var reader = cmd.ExecuteReader())
                    {
                        dataTable.Load(reader);
                    }
                }

                if (!wasOpen)
                    connection.Close();
            }
            catch (Exception ex)
            {
                dataTable = null;
                Console.WriteLine("Error executing data reader: " + ex.Message);
            }

            return dataTable;
        }

        public static void TestFirebirdConnection()
        {
            // Set the ServerType to 1 for connect to the embedded server
            string connectionString =
                "User=SYSDBA;" +
                "Password=masterkey;" +
                @"Database=C:\Users\henry\Documents\Clients\GSOC\Data\CAPE\2018 Full Model;"; //+
                //"DataSource=localhost;" +
                //"Port=3050;" +
                //"Dialect=3;" +
                //"Charset=NONE;" +
                //"Role=;" +
                //"Connection lifetime=15;" +
                //"Pooling=true;" +
                //"MinPoolSize=0;" +
                //"MaxPoolSize=50;" +
                //"Packet Size=8192;" +
                //"ServerType=0";

            FbConnection myConnection1 = new FbConnection(connectionString);

            try
            {
                myConnection1.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening database connection: " + ex.Message);
            }

        }


    }
}