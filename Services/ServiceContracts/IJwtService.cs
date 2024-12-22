using Core.Dto;

namespace Core.ServiceContracts
{
    public interface IJwtService
    {
        public string CreateToken(UserDto user);
    }
}
