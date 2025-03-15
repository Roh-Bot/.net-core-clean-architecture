using System.Collections.Concurrent;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using Domain.Entities;
using Domain.RepositoryContracts;
using Domain.Globals;
using Infrastructure.Constants;

namespace Infrastructure.Repositories
{
    public class UserRepository(IDatabaseFactory dbFactory) : IUserRepository
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
            var parameters = new DynamicParameters();
            parameters.Add("@Username", user.Username);


            return await connection.QueryFirstAsync<User>("UsersRead", parameters, commandType: CommandType.StoredProcedure);
        }

        public string GetTokenUserVersion(string email)
        {
            if (Users.UserVersions.TryGetValue(email, out var version))
            {
                return version;
            }

            // Initialize version if it doesn't exist
            Users.UserVersions[email] = "1";
            return "1";
        }

        public void IncrementTokenUserVersion(string email)
        {
            if (Users.UserVersions.TryGetValue(email, out var version))
            {
                Users.UserVersions[email] = (int.Parse(version) + 1).ToString();
                return;
            }
            Users.UserVersions[email] = "1";
        }
    }
}
