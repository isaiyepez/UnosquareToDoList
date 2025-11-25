using Entities;
using Entities.DTOs;

namespace BusinessLogic.Contracts
{
    public interface IUserService
    {
        Task<UserDto?> AddUserAsync(RegisterDto registerDto);
        Task<User?> GetUserFromEmailAsync(string email);
        Task UpdateUserAsync(UserDto userDto);
        Task DeleteUserAsync(int userId);
    }
}
