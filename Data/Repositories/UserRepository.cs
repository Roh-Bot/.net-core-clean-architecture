using Data.Entities;
using Data.RepositoryContracts;

namespace Data.Repositories
{
    public class UserRepository(DatabaseConnectionFactory dbConnection) : IUserRepository
    {
        public async Task Create(User user)
        {
            await using var connection = dbConnection.CreateConnection();
        }

        public async Task<User> Read(User user)
        {
            await using var connection = dbConnection.CreateConnection();
            return new User() { Username = user.Username };
        }
    }
}
