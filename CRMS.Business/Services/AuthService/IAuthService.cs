
using CRMS.Domain.Entities;

namespace CRMS.Business.Services.AuthService
{
    public interface IAuthService
    {
        User CurrentUser { get; }
        Task<User?> AuthenticateAsync(string login, string password);
        Task RegisterAsync(User user);
        Task<bool> UserEmailExists(string email);
        Task<bool> UserNameExists(string firstName, string lastName);
        Task<User?> GetFirstAdminAsync();
        Task<User?> GetUserByEmailAsync(string email);
        void Logout();
    }
}
