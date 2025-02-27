using UserManagementApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
 
namespace UserManagementApi.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserById(int id);
        Task<User> GetUserByUsername(string username);
        Task<User> GetUserByRefreshToken(string refreshToken);
        Task<IEnumerable<User>> GetAllUsers();
        Task AddUser(User user);
        Task UpdateUser(User user);
        Task DeleteUser(int id);
    }
}