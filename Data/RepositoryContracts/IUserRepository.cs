using Data.Entities;

namespace Data.RepositoryContracts
{
    public interface IUserRepository
    {
        Task Create(User u);
        Task<User> Read(User u);
    }
}
