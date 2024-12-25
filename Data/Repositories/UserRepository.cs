using Data.Constants;
using Data.Entities;
using Data.RepositoryContracts;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Data.Repositories
{
    public class UserRepository(DatabaseFactory dbFactory) : IUserRepository
    {
        public async Task Create(User user)
        {
            await using var connection = dbFactory.CreateConnection();
            using var ctsConnection = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await connection.OpenAsync(ctsConnection.Token);

            await using var command = new SqlCommand()
            {
                Connection = connection,
                CommandText = UserConstants.SpName,
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30,
                RetryLogicProvider = dbFactory.RetryLogic()
            };
            using var ctsQuery = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await command.ExecuteNonQueryAsync(ctsQuery.Token);
        }

        public async Task<User> Read(User user)
        {
            await using var connection = dbFactory.CreateConnection();
            return new User() { Username = user.Username };
        }
    }
}
