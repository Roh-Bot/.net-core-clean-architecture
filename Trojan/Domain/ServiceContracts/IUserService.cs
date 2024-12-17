using Domain.Entities;
namespace Domain.ServiceContracts
{
    public interface IUserService
    {
        User Create(User user);
        User Read(User user);
    }
}
