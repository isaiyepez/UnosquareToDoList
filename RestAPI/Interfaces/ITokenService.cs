using Entities;

namespace RestAPI.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
