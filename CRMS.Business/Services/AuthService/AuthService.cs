using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;

namespace CRMS.Business.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private User _currentUser;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public User CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
            }
        }

        public async Task<User?> AuthenticateAsync(string login, string password)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(login);
            if (user == null)
                return null;

            if (user.ValidatePassword(password))
            {
                CurrentUser = user;
                return CurrentUser;
            }

            return null;
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        public async Task RegisterAsync(User user)
        {
            if (await UserEmailExists(user.Email))
                throw new Exception("Пользователь с такой почтой уже существует.");
            if (await UserNameExists(user.FirstName, user.LastName))
                throw new Exception("Пользователь с таким именем уже существует.");
            await _unitOfWork.Users.AddAsync(user);
        }

        public async Task<bool> UserEmailExists(string login)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(login);
            return user == null;
        }

        public async Task<bool> UserNameExists(string firstName, string lastName)
        {
            var user = await _unitOfWork.Users.GetByNameAsync(firstName, lastName);
            return user != null;
        }

        public async Task<User?> GetFirstAdminAsync()
        {
            var admins = await _unitOfWork.Users.GetUsersByRoleAsync(UserRole.Admin);
            return admins?.FirstOrDefault();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var users = await _unitOfWork.Users.GetWhereAsync(u => u.Email == email);
            return users.FirstOrDefault();
        }
    }
}
