using Dapper;
using Microsoft.Data.SqlClient;
using Project.Core.Interfaces;
using Project.Domain.Entities;
using System.Data;

namespace Project.Infrastructure.Repositories;

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
            CommandText = "SpName",
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
}