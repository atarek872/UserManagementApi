using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using UserManagementApi.Application.Services;
using UserManagementApi.Data;
using UserManagementApi.Domain.Entities;
using UserManagementApi.Infrastructure.Repositories;
using UserManagementApi.DTOs;
using UserManagementApi.Infrastructure;
using UserManagementApi.Models;

namespace UserManagementApi.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly AppSettings _appSettings;
        private readonly UserManagementDbContext _context;

        public UserService(IUserRepository userRepository, IOptions<AppSettings> appSettings, UserManagementDbContext context)
        {
            _userRepository = userRepository;
            _appSettings = appSettings.Value;
            _context = context;
        }

        public async Task<User> GetUserById(int id)
        {
            return await _userRepository.GetUserById(id);
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _userRepository.GetUserByUsername(username);
        }

        public async Task<User> GetUserByRefreshToken(string refreshToken)
        {
            return await _userRepository.GetUserByRefreshToken(refreshToken);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _userRepository.GetAllUsers();
        }

        public async Task AddUser(User user)
        {
            // Hash the password before saving
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _userRepository.AddUser(user);
        }

        public async Task UpdateUser(User user)
        {
            await _userRepository.UpdateUser(user);
        }

        public async Task DeleteUser(int id)
        {
            await _userRepository.DeleteUser(id);
        }

        public async Task<AuthResponse> Authenticate(string username, string password)
        {
            var user = await _userRepository.GetUserByUsername(username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            var jwtToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Set expiry time for refresh token
            await _userRepository.UpdateUser(user);

            return new AuthResponse
            {
                Username = user.Username,
                JwtToken = jwtToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponse> RefreshToken(string token)
        {
            var user = await _userRepository.GetUserByRefreshToken(token);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return null;

            var jwtToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Set expiry time for refresh token
            await _userRepository.UpdateUser(user);

            return new AuthResponse
            {
                Username = user.Username,
                JwtToken = jwtToken,
                RefreshToken = refreshToken
            };
        }

        public async Task AddRoleToUser(int userId, int roleId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
                throw new KeyNotFoundException("Role not found");

            user.UserRoles.Add(new UserRole { Role = role });
            await _userRepository.UpdateUser(user);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    // Add more claims if needed
                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}