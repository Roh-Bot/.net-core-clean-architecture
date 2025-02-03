
using Domain.Entities;

namespace Domain.RepositoryContracts
{
    public interface IUserRepository
    {
        Task Create(User u);
        Task<User> Read(User u);
        string GetTokenUserVersion(string email);
        void IncrementTokenUserVersion(string email);
    }
}
