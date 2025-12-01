using Entities;
using Entities.DTOs;

namespace BusinessLogic.Contracts
{
    public interface IUserService
    {
        Task<UserDto?> AddUserAsync(RegisterDto registerDto);
        Task<User?> GetUserFromIdAsync(int id);
        Task<UserDto?> ValidateUserCredentialsAsync(string email, string password);
        Task UpdateUserAsync(UserDto userDto);
        Task<bool> DeleteUserAsync(int userId);
    }
}
