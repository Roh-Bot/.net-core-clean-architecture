using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Data
{
    public class DatabaseConnectionFactory(IConfiguration configuration)
    {
        public SqlConnection CreateConnection()
        {
            Console.Write(configuration.GetConnectionString("DefaultConnection"));
            return new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

    }
}
