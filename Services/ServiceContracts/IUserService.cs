using Core.Dto;
using Data.Entities;

namespace Core.ServiceContracts
{
    public interface IUserService
    {
        Task Create(UserDto u);
        Task<User> Read(UserDto u);
    }
}
