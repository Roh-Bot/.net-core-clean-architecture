using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Infrastructure
{
    public class DatabaseFactory(IConfiguration configuration) : IDatabaseFactory
    {
        public static readonly List<int> SqlServerTransientErrors =
        [
            10928, 10929, // Resource limits
            1205, // Deadlock
            233, // Connection terminated
            10053, 10054, 10060, // Network errors
            11001 // Host not found
        ];
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
                TransientErrors = SqlServerTransientErrors
            };
            return SqlConfigurableRetryFactory.CreateExponentialRetryProvider(options);
        }
    }
}
