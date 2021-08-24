using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using DataAccess.Models;

namespace DataAccess.DataOperations
{
    public class InventoryDal:IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection _sqlConnection = null;
        bool _disposed = false;

        public InventoryDal() : this(@"Data Source= VachmirLaptop;Integrated Security = true; Initial Catalog=AutoLot")
        {
        }

        public InventoryDal(string connectionString)
        {
            this._connectionString = connectionString;
        }

        private void OpenConnection()
        {
            _sqlConnection = new SqlConnection { ConnectionString = _connectionString };
            _sqlConnection.Open();
        }

        private void CloseConnection()
        {
            if (_sqlConnection?.State!=ConnectionState.Closed)
            {
                _sqlConnection?.Close();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                _sqlConnection.Dispose();
            }
            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public List<CarViewModel> GetAllInventory()
        {
            OpenConnection();
            // This will hold the records.
            List<CarViewModel> inventory = new List<CarViewModel>();
            // Prep command object.
            string sql =
            @"SELECT i.Id, i.Color, i.PetName,m.Name as Make FROM Inventory i INNER JOIN Makes m on m.Id = i.MakeId";
            using SqlCommand command =
            new SqlCommand(sql, _sqlConnection)
            {
                CommandType = CommandType.Text
            };
            command.CommandType = CommandType.Text;
            SqlDataReader dataReader =
            command.ExecuteReader(CommandBehavior.CloseConnection);
            while (dataReader.Read())
            {
                inventory.Add(new CarViewModel
                {
                    Id = (int)dataReader["Id"],
                    Color = (string)dataReader["Color"],
                    Make = (string)dataReader["Make"],
                    PetName = (string)dataReader["PetName"]
                });
            }
            dataReader.Close();
            return inventory;
        }

        public CarViewModel GetCar(int id)
        {
            OpenConnection();
            CarViewModel? car = null;
            //This should use parameters for security reasons
            string sql =$@"SELECT i.Id, i.Color, i.PetName,m.Name as Make FROM Inventory i INNER JOIN Makes m on m.Id = i.MakeId WHERE i.Id = {id}";
            using SqlCommand command =
            new SqlCommand(sql, _sqlConnection)
            {
                CommandType = CommandType.Text
            };
            SqlDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            while (dataReader.Read())
            {
                car = new CarViewModel
                {
                    Id = (int)dataReader["Id"],
                    Color = (string)dataReader["Color"],
                    Make = (string)dataReader["Make"],
                    PetName = (string)dataReader["PetName"]
                };
            }
            dataReader.Close();
            return car;
        }

        public static void InsertAuto(string color, int makeId, string petName)
        {
            OpenConnection();
            //Format a SQL statement
            string sql = $"Insert Into Inventory (MakeId, Color, PetName) Values ('{makeId}', '{color}', '{petName}')";

            //Execute with connection
            using(SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
            CloseConnection();
        }

        public void InsertAuto(Car car)
        {
            OpenConnection();
            // 
            string sql = "Insert Into Inventory (MakeId, Color, PetName) Values " +
            "(@MakeId, @Color, @PetName)";
            // Execute with connection.
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName="@MakeId",
                    Value=car.MakeId,
                    SqlDbType=SqlDbType.Int,
                    Direction=ParameterDirection.Input
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@Color",
                    Value = car.Color,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 50,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@PetName",
                    Value = car.PetName,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 50,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(parameter);
                command.ExecuteNonQuery();
                CloseConnection();
            }
            
        }

        public void DeleteCar(int id)
        {
            OpenConnection();
            //Get the ID of car and delete
            string sql = $"Delete from Inventory where Id = '{id}'";
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                try
                {
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Exception error = new Exception("Sorry! Car is in order");
                    throw error;
                }
            }
            CloseConnection();
        }

        public void UpdateCarPetName(int id, string newPetName)
        {
            OpenConnection();
            // Get ID of car to modify the pet name.
            string sql = $"Update Inventory Set PetName = '{newPetName}' Where Id = '{id}'";
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.ExecuteNonQuery();
            }
            CloseConnection();
        }

        public void ProcessCreditRisk(bool throwEx, int customerId)
        {
            OpenConnection();
            string fName, lName;
            var cmdSelect = new SqlCommand("Select * from Customers where Id = @customerId", _sqlConnection);
            SqlParameter paramId = new SqlParameter
            {
                ParameterName = "@customerId",
                SqlDbType = SqlDbType.Int,
                Value = customerId,
                Direction = ParameterDirection.Input
            };
            cmdSelect.Parameters.Add(paramId);
            using (var dataReader=cmdSelect.ExecuteReader())
            {
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    fName = (string)dataReader["FirstName"];
                    lName= (string)dataReader["LastName"];
                }
                else
                {
                    CloseConnection();
                    return;
                }
            }
            cmdSelect.Parameters.Clear();

            // Command objects representing each step of the operation.
            var cmdUpdate = new SqlCommand("Update Customers set LastName = LastName + ' (CreditRisk) ' where Id = @customerId", _sqlConnection);
            cmdUpdate.Parameters.Add(paramId);

            var cmdInsert = new SqlCommand("Insert Into CreditRisks (CustomerId,FirstName, LastName) Values( @CustomerId, @FirstName, @LastName)", _sqlConnection);
            SqlParameter parameterId2 = new SqlParameter
            {
                ParameterName = "@CustomerId",
                SqlDbType = SqlDbType.Int,
                Value = customerId,
                Direction = ParameterDirection.Input
            };
            SqlParameter parameterFirstName = new SqlParameter
            {
                ParameterName = "@FirstName",
                Value = fName,
                SqlDbType = SqlDbType.NVarChar,
                Size = 50,
                Direction = ParameterDirection.Input
            };
            SqlParameter parameterLastName = new SqlParameter
            {
                ParameterName = "@LastName",
                Value = lName,
                SqlDbType = SqlDbType.NVarChar,
                Size = 50,
                Direction = ParameterDirection.Input
            }; 
            cmdInsert.Parameters.Add(parameterId2);
            cmdInsert.Parameters.Add(parameterFirstName);
            cmdInsert.Parameters.Add(parameterLastName);

            SqlTransaction tx = null;
            try
            {
                tx = _sqlConnection.BeginTransaction();

                cmdInsert.Transaction = tx;
                cmdUpdate.Transaction = tx;

                cmdInsert.ExecuteNonQuery();
                cmdUpdate.ExecuteNonQuery();

                if (throwEx)
                {
                    throw new Exception("Sorry! Database error! Tx failed!");
                }

                tx.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                tx?.Rollback();
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}
