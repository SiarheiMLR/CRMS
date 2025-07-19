using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using ControlzEx.Standard;

namespace CRMS.Business.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task AddUserAsync(User user)
        {
            try
            {
                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync(); // Сохраняем изменения
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.InnerException?.Message}");
            }
        }

        public async Task<User?> AuthenticateUserAsync(string email, string password)
        {
            // Ищем пользователя по email
            var user = (await FindUsersAsync(u => u.Email == email)).FirstOrDefault();

            if (user == null)
                return null; // Если пользователь не найден

            // Проверяем пароль
            if (ValidatePassword(password, user.PasswordHash, user.PasswordSalt))
                return user; // Если пароль верный, возвращаем пользователя

            return null; // Если пароль неверный
        }

        public void DeleteUser(User user)
        {
            _unitOfWork.Users.Remove(user);
            _unitOfWork.SaveChanges();
        }

        public async Task<IEnumerable<User>> FindUsersAsync(Expression<Func<User, bool>> predicate)
        {
            return await _unitOfWork.Users.GetWhereAsync(predicate);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            // Загружаем пользователей с группами и связями ролей
            return await _unitOfWork.Users.GetAllAsync(
                "GroupMembers",
                "GroupMembers.Group",
                "GroupMembers.Group.GroupRoleMapping"
            );
        }

        public async Task<IEnumerable<User>> GetUsersByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetWhereAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<IEnumerable<User>> GetUsersByLoginAsync(string login)
        {
            return await _unitOfWork.Users.GetWhereAsync(u => u.UserLogonName == login);
        }

        public async Task<IEnumerable<User>> GetUsersByNameAsync(string firstName, string lastName)
        {
            return await _unitOfWork.Users
                .GetWhereAsync(u => u.FirstName == firstName && u.LastName == lastName);
        }

        public void UpdateUser(User user)
        {
            _unitOfWork.Users.Update(user);
            _unitOfWork.SaveChanges();
        }

        public bool ValidatePassword(string password, string storedHash, string storedSalt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] combinedBytes = Encoding.UTF8.GetBytes(password + storedSalt);
                byte[] hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes) == storedHash;
            }
        }       
    }
}
