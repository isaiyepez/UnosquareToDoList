using BusinessLogic.Contracts;
using Entities;
using Entities.DTOs;

namespace BusinessLogic.Extensions
{
    public static class UserExtensions
    {
        public static UserDto ToDto(this User user, ITokenService tokenService)
        {
            return new UserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = tokenService.CreateToken(user)
            };
        }
    }
}
