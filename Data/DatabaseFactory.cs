using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Data
{
    public class DatabaseFactory(IConfiguration configuration)
    {
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        public SqlRetryLogicBaseProvider RetryLogic()
        {
            var options = new SqlRetryLogicOption()
            {
                NumberOfTries = Convert.ToInt32(configuration["Retry:NumberOfTries"]),
                MaxTimeInterval = TimeSpan.FromSeconds(20),
                DeltaTime = TimeSpan.FromSeconds(1),
                TransientErrors =
                [
                    40613, 40197, 40501, 49918, 49919, 49920, // Azure SQL errors
                    10928, 10929, // Resource limits
                    1205, // Deadlock
                    233, // Connection terminated
                    10053, 10054, 10060, // Network errors
                    11001 // Host not found
                ]
            };
            return SqlConfigurableRetryFactory.CreateExponentialRetryProvider(options);
        }
    }
}
