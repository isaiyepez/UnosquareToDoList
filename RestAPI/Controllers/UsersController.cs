using BusinessLogic.Contracts;
using BusinessLogic.Extensions;
using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public UsersController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                return BadRequest("Email and password must be provided.");

            var userDto = await _userService.ValidateUserCredentialsAsync(loginDto.Email, loginDto.Password);

            if (userDto == null)
            {
                return Unauthorized("Invalid credentials");
            }

            return userDto;
        }


        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> PatchUser(int id, [FromBody] JsonPatchDocument<UserDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var user = await _userService.GetUserFromIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userDto = user.ToDto(_tokenService);

            patchDoc.ApplyTo(userDto, ModelState);

            if (!TryValidateModel(userDto))
            {
                return ValidationProblem(ModelState);
            }

            await _userService.UpdateUserAsync(userDto);

            return NoContent();
        }


        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUser(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userDto = await _userService.AddUserAsync(registerDto);

            if (userDto == null)
            {
                // User already exists
                return Conflict("A user with this email already exists.");
            }

            // Return 201 Created
            return Created(string.Empty, userDto);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetUserFromIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }


            if (!await _userService.DeleteUserAsync(id))
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}
