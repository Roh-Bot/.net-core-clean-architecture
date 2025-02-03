using Core.Dto;
using Domain.Entities;

namespace Core.ServiceContracts
{
    public interface IUserService
    {
        Task Create(UserDto u);
        Task<User> Read(UserDto u);
    }
}
