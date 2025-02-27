using UserManagementApi.Domain.Entities;
using UserManagementApi.DTOs;

namespace UserManagementApi.Application.Services
{
    public interface IUserService
    {
        Task<User> GetUserById(int id);
        Task<User> GetUserByUsername(string username);
        Task<User> GetUserByRefreshToken(string refreshToken);
        Task<IEnumerable<User>> GetAllUsers();
        Task AddUser(User user);
        Task UpdateUser(User user);
        Task DeleteUser(int id);
        Task<AuthResponse> Authenticate(string username, string password);
        Task<AuthResponse> RefreshToken(string token);
        Task AddRoleToUser(int userId, int roleId);
    }
}