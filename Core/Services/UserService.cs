using Core.Dto;
using Core.ServiceContracts;
using Domain.Entities;
using Domain.RepositoryContracts;

namespace Core.Services
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        public async Task Create(UserDto u)
        {
            var user = new User()
            {
                Username = u.Username

            };
            //await userRepository.Create(user);
        }

        public async Task<User> Read(UserDto u)
        {
            var userRepo = new User()
            {
                Username = u.Username,
            };
            return await userRepository.Read(userRepo);
        }
    }
}
