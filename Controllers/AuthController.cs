using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UserManagementApi.Application.Services;
using UserManagementApi.Domain.Entities;
using UserManagementApi.DTOs;

namespace UserManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var authResponse = await _userService.Authenticate(model.Username, model.Password);
            if (authResponse == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(authResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (await _userService.GetUserByUsername(model.Username) != null)
                return BadRequest(new { message = "Username already exists" });

            var user = new User
            {
                Username = model.Username,
                PasswordHash = model.Password
            };

            await _userService.AddUser(user);
            return Ok(new { message = "Registration successful" });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Implement logout logic if needed
            return Ok(new { message = "Logout successful" });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            var authResponse = await _userService.RefreshToken(model.Token);
            if (authResponse == null)
                return Unauthorized(new { message = "Invalid token" });

            return Ok(authResponse);
        }
    }
}
 