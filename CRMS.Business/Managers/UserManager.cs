using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CRMS.Business.Managers
{
    public class UserManager : BaseManager
    {
        public UserManager(IUnitOfWork unitOfWork) : base(unitOfWork) { }

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

        // Метод проверки пароля
        private bool ValidatePassword(string password, string storedHash, string storedSalt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] combinedBytes = Encoding.UTF8.GetBytes(password + storedSalt);
                byte[] hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes) == storedHash;
            }
        }

        // Получение всех пользователей
        public async Task<IEnumerable<User>> GetAllUsersAsync() => await _unitOfWork.Users.GetAllAsync();
        // Получение пользователя по Id
        public async Task<User> GetUserByIdAsync(int id) => await _unitOfWork.Users.GetByIdAsync(id);

        // Добавление нового пользователя
        //public async Task AddUserAsync(User user) => await userRepository.AddAsync(user); // Добавление без сохранения
        public async Task AddUserAsync(User user)
        {
            // Чтобы увидеть точную причину ошибки
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
        // Обновление существующего пользователя
        public void UpdateUser(User user)
        {
            _unitOfWork.Users.Update(user);
            _unitOfWork.SaveChanges(); // Сохраняем изменения
        }
        // Удаление пользователя
        public void DeleteUser(User user)
        {
            _unitOfWork.Users.Remove(user);
            _unitOfWork.SaveChanges(); // Сохраняем изменения
        }

        // Поиск пользователей по предикату
        public async Task<IEnumerable<User>> FindUsersAsync(Expression<Func<User, bool>> predicate)
            => await _unitOfWork.Users.GetWhereAsync(predicate);
    }
}
