using System.Data;
using System.Data.Common;
using System.Data.Odbc;
#if PC 
  using System.Data.OleDb;
#endif
using Microsoft.Data.SqlClient;

namespace DataAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** Very Simple Connection Factory *****\n");
            Setup(DataProviderEnum.SqlServer);
#if PC
            Setup(DataProviderEnum.OleDb); //Not supported on macOS
#endif
            Setup(DataProviderEnum.Odbc);
            //Setup(DataProviderEnum.None);
            Console.ReadLine();
        }

        static void Setup(DataProviderEnum provider)
        {
            IDbConnection con =  GetConnection(provider);
            Console.WriteLine($"Your connection is a {con.GetType().Name ?? "unrecognised type"}");
        }

       static IDbConnection GetConnection(DataProviderEnum dataProvider)
      => dataProvider switch
      {
          DataProviderEnum.SqlServer => new SqlConnection(),
       #if PC
          DataProviderEnum.OleDb => new OleDbConnection(),
       #endif
          DataProviderEnum.Odbc=> new OdbcConnection(),
          //_  =>null,
        };
    }
}