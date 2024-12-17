using Domain.Entities;
using Domain.ServiceContracts;
namespace Services
{
    public class UserService : IUserService
    {
        public User Create(User user)
        {
            return new User();
        }
        public User Read(User user)
        {
            return new User();
        }
    }
}
