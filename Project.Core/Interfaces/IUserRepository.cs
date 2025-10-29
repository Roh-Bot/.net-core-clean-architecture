using Project.Domain.Entities;

namespace Project.Core.Interfaces
{
    public interface IUserRepository
    {
        Task Create(User u);
        Task<User> Read(User u);
    }
}
