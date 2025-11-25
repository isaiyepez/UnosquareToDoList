using Entities;

namespace BusinessLogic.Contracts
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
