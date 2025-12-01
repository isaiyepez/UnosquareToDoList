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
                Email = registerDto.Email.ToLowerInvariant(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user.ToDto(_tokenService);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return false;

            _context.Users.Remove(user);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                // Log exception or handle conflicts
                return false;
            }
        }


        public async Task<UserDto?> ValidateUserCredentialsAsync(string email, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            if (!CryptographicOperations.FixedTimeEquals(computedHash, user.PasswordHash))
            {
                return null;
            }

            return user.ToDto(_tokenService);
        }

        public async Task<User?> GetUserFromIdAsync(int id)
        {
            return await _context.Users
                .SingleOrDefaultAsync(user => user.Id == id);
        }

        public async Task UpdateUserAsync(UserDto userDto)
        {
            var user = await _context.Users.FindAsync(userDto.Id);
            if (user == null) return;

            user.DisplayName = userDto.DisplayName ?? user.DisplayName;

            _context.Users.Update(user);

            await _context.SaveChangesAsync();
        }

        private async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}
