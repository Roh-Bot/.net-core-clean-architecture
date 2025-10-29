using Project.Core.Dto;
using Project.Core.Interfaces;
using Project.Domain.Entities;

namespace Project.Core.Application
{
    public class UserUseCase(IUserRepository userRepository)
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
