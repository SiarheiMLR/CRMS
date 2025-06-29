
using CRMS.Domain.Entities;
using System.Linq.Expressions;

namespace CRMS.Business.Services.UserService
{
    public interface IUserService
    {
        public Task<User?> AuthenticateUserAsync(string email, string password);
        public bool ValidatePassword(string password, string storedHash, string storedSalt);
        public Task<IEnumerable<User>> GetAllUsersAsync();
        public Task<User> GetUserByIdAsync(int id);
        public Task<IEnumerable<User>> GetUsersByNameAsync(string firstName, string lastName);
        public Task<IEnumerable<User>> GetUsersByEmailAsync(string email);
        public Task<IEnumerable<User>> GetUsersByLoginAsync(string login);
        public Task AddUserAsync(User user);
        public void UpdateUser(User user);
        public void DeleteUser(User user);
        public Task<IEnumerable<User>> FindUsersAsync(Expression<Func<User, bool>> predicate);
    }
}
