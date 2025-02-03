using Microsoft.Data.SqlClient;

namespace Infrastructure
{
    public interface IDatabaseFactory
    {
        public SqlConnection CreateConnection();
        public SqlRetryLogicBaseProvider RetryLogic();
    }
}
