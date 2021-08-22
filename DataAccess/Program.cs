using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
#if PC 
  using System.Data.OleDb;
#endif
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DataAccess 
{
    class Program
    {
        static void Main(string[] args)
        {        

            // DataProviderFactory();
            //SQLConnection();
            ConnectionStringBuilder();
        }
        static void DataProviderFactory()
        {
            var (provider, connectionString) = GetProviderFromConfiguration();
            DbProviderFactory? factory = GetDbProviderFactory(provider);

            //#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            using (DbConnection connection = factory.CreateConnection())
            //#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            {
                if (connection == null)
                {
                    Console.WriteLine($"Unable to create the connection object");
                    return;
                }

                Console.WriteLine($"Your connection object is a: {connection.GetType().Name}");
                connection.ConnectionString = connectionString;
                connection.Open();

                DbCommand? command = factory.CreateCommand();
                if (command == null)
                {
                    Console.WriteLine($"Unable to create the command object");
                    return;
                }

                Console.WriteLine($"Your command object is a: {command.GetType().Name}");
                command.Connection = connection;
                command.CommandText = "Select i.Id, m.Name From Inventory i inner join Makes m on m.Id = i.MakeId ";


                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    Console.WriteLine($"Your data reader object is a: {dataReader.GetType().Name}");
                    while (dataReader.Read())
                    {
                        Console.WriteLine($"Car {dataReader["Id"]} is {dataReader["Name"]}.");
                    }
                }
            }
        }
        static DbProviderFactory? GetDbProviderFactory(DataProviderEnum provider) => provider switch
             {
                 DataProviderEnum.SqlServer => SqlClientFactory.Instance,
                 DataProviderEnum.Odbc => OdbcFactory.Instance,
              #if PC
                 DataProviderEnum.OleDb => OleDbFactory.Instance,
              #endif
                 _ => null
             };
        static (DataProviderEnum Provider, string ConnectioString) GetProviderFromConfiguration()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            var providerName = config["ProviderName"];
            if (Enum.TryParse<DataProviderEnum>(providerName, out DataProviderEnum provider))
            {
                return (provider, config[$"{providerName}:ConnectionString"]);
            };
            throw new Exception("Invalid Data Provider value supplied");
        }
    
        static void SQLConnection()
        {
            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = @" Data Source=.;Integrated Security = True;Initial Catalog=AutoLot";
                connection.Open();
                // Create a SQL command object.
                string sql = @"Select i.id, m.Name as Make, i.Color, i.Petname FROM Inventory i INNER JOIN Makes m on m.Id = i.MakeId";
                SqlCommand myCommand = new SqlCommand(sql, connection);
                // Obtain a data reader a la ExecuteReader().
                using (SqlDataReader myDataReader = myCommand.ExecuteReader())
                {
                    // Loop over the results.
                    while (myDataReader.Read())
                    {
                        Console.WriteLine($"-> Make: {myDataReader["Make"]}, PetName: {myDataReader["PetName"]}, Color: {myDataReader["Color"]}.");
                    }
                }
            }
        }

        static void ConnectionStringBuilder()
        {
            using (SqlConnection connection = new SqlConnection())
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder
                {
                    InitialCatalog = "AutoLot",
                    DataSource = "VachmirLaptop",
                    IntegratedSecurity = true,
                    ConnectTimeout = 30
                };
                connection.ConnectionString = connectionStringBuilder.ConnectionString;
                connection.Open();
                // Create a SQL command object.
                string sql = @"Select i.id, m.Name as Make, i.Color, i.Petname FROM Inventory i INNER JOIN Makes m on m.Id = i.MakeId";
                SqlCommand myCommand = new SqlCommand(sql, connection);
                // Obtain a data reader a la ExecuteReader().
                using (SqlDataReader myDataReader = myCommand.ExecuteReader())
                {
                    // Loop over the results.
                    while (myDataReader.Read())
                    {
                        Console.WriteLine($"-> Make: {myDataReader["Make"]}, PetName: {myDataReader["PetName"]}, Color: {myDataReader["Color"]}.");
                    }
                }
            }         

        }
    }
}
