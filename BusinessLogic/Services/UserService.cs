using BusinessLogic.Contracts;
using BusinessLogic.Extensions;
using Entities;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using Data;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLogic.Services
{
    
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public UserService(AppDbContext dbContext, ITokenService tokenService)
        {
            _context = dbContext;
            _tokenService = tokenService;
        }

        public async Task<UserDto?> AddUserAsync(RegisterDto registerDto)
        {
            if (await UserExistsAsync(registerDto.Email))
            {
                return null;
            }

            using var hmac = new HMACSHA512();

            var user = new User
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user.ToDto(_tokenService);
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User?> GetUserFromEmailAsync(string email)
        {
            return await _context.Users
                 .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task UpdateUserAsync(UserDto userDto)
        {
            var user = await _context.Users.FindAsync(userDto.Id);

            if (user != null)
            {
                user.DisplayName = userDto.DisplayName;

                _context.Users.Update(user);

                await _context.SaveChangesAsync();
            }
        }

        private async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}
