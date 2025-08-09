using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using ControlzEx.Standard;
using CRMS.Business.Services.EmailService;
using CRMS.Business.Services.EmailService.Templates;

namespace CRMS.Business.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public UserService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task AddUserAsync(User user)
        {
            try
            {
                if (user.AccountCreated == default)
                    user.AccountCreated = DateTime.UtcNow;

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync(); // Сохраняем изменения

                // 📤 Отправляем письмо пользователю
                var userParams = new Dictionary<string, string>
                {
                    { "FirstName", user.FirstName ?? "пользователь" },
                    { "Email", user.UserLogonName },
                    { "Password", user.InitialPassword ?? "неизвестен" }
                };

                await _emailService.SendTemplateAsync(
                    to: user.UserLogonName,
                    subject: "Добро пожаловать в CRMS",
                    template: Templates.UserCreatedFromAD,
                    parameters: userParams
                );

                // 📤 Уведомление администратору
                var adminParams = new Dictionary<string, string>
                {
                    { "DisplayName", user.DisplayName ?? $"{user.FirstName} {user.LastName}" },
                    { "Email", user.UserLogonName },
                    { "Password", user.InitialPassword ?? "неизвестен" },
                    { "Source", "Active Directory" },
                    { "Date", DateTime.UtcNow.ToString("g") }
                };

                await _emailService.SendTemplateAsync(
                    to: "admin@bigfirm.by", // Можно заменить на список из конфигурации
                    subject: $"[CRMS] Добавлен новый пользователь из AD",
                    template: Templates.AdminNotificationUserCreated,
                    parameters: adminParams
                );
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.InnerException?.Message}");
            }
        }

        public async Task<User> AddUserAsyncManual(User user)
        {
            try
            {
                if (user.AccountCreated == default)
                    user.AccountCreated = DateTime.UtcNow;

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                return user; // Возвращаем созданного пользователя

                // 📤 Отправляем письмо пользователю
                var userParams = new Dictionary<string, string>
                {
                    { "FirstName", user.FirstName ?? "пользователь" },
                    { "Email", user.UserLogonName },
                    { "Password", user.InitialPassword ?? "неизвестен" }
                };

                await _emailService.SendTemplateAsync(
                    to: user.UserLogonName,
                    subject: "Добро пожаловать в CRMS",
                    template: Templates.UserCreatedFromAD,
                    parameters: userParams
                );

                // 📤 Уведомление администратору
                var adminParams = new Dictionary<string, string>
                {
                    { "DisplayName", user.DisplayName ?? $"{user.FirstName} {user.LastName}" },
                    { "Email", user.UserLogonName },
                    { "Password", user.InitialPassword ?? "неизвестен" },
                    { "Source", "Active Directory" },
                    { "Date", DateTime.UtcNow.ToString("g") }
                };

                await _emailService.SendTemplateAsync(
                    to: "admin@bigfirm.by", // Можно заменить на список из конфигурации
                    subject: $"[CRMS] Добавлен новый пользователь из AD",
                    template: Templates.AdminNotificationUserCreated,
                    parameters: adminParams
                );
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.InnerException?.Message}");
                return null;
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

        //public async Task DeleteUserAsync(User user)
        //{
        //    _unitOfWork.Users.Remove(user);
        //    await _unitOfWork.SaveChangesAsync();
        //}

        public async Task DeleteUserAsync(User user)
        {
            try
            {
                // Загружаем пользователя из базы по Id
                var userInDb = await _unitOfWork.Users.GetByIdAsync(user.Id);
                if (userInDb == null)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем наличие тикетов, где он Requestor или Supporter
                var hasLinkedTickets = (await _unitOfWork.Tickets.GetWhereAsync(t =>
                    t.RequestorId == user.Id || t.SupporterId == user.Id)).Any();

                if (hasLinkedTickets)
                {
                    MessageBox.Show("Невозможно удалить пользователя: он связан с одной или несколькими заявками.",
                                    "Удаление невозможно",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Удаление пользователя
                _unitOfWork.Users.Remove(userInDb);
                await _unitOfWork.SaveChangesAsync();

                MessageBox.Show("Пользователь успешно удалён.",
                                "Удаление завершено",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка при удалении пользователя: {ex.InnerException?.Message ?? ex.Message}",
                                "Ошибка БД",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            return await _unitOfWork.Users.GetByIdAsync(id, "GroupMembers", "GroupMembers.Group");
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

        //public async Task UpdateUserAsync(User user)
        //{
        //    _unitOfWork.Users.Update(user);
        //    await _unitOfWork.SaveChangesAsync();
        //}

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                // Загружаем оригинального пользователя из базы данных
                var userInDb = await _unitOfWork.Users.GetByIdAsync(user.Id);
                if (userInDb == null)
                {
                    MessageBox.Show("Пользователь не найден в базе данных.",
                                    "Ошибка обновления",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Обновляем все поля (ручное копирование)
                userInDb.FirstName = user.FirstName;
                userInDb.LastName = user.LastName;
                userInDb.Initials = user.Initials;
                userInDb.DisplayName = user.DisplayName;
                userInDb.Description = user.Description;
                userInDb.Office = user.Office;
                userInDb.Email = user.Email;
                userInDb.WebPage = user.WebPage;
                userInDb.DateOfBirth = user.DateOfBirth;
                userInDb.Street = user.Street;
                userInDb.City = user.City;
                userInDb.State = user.State;
                userInDb.PostalCode = user.PostalCode;
                userInDb.Country = user.Country;
                userInDb.UserLogonName = user.UserLogonName;
                userInDb.WorkPhone = user.WorkPhone;
                userInDb.MobilePhone = user.MobilePhone;
                userInDb.IPPhone = user.IPPhone;
                userInDb.JobTitle = user.JobTitle;
                userInDb.Department = user.Department;
                userInDb.Company = user.Company;
                userInDb.ManagerName = user.ManagerName;
                userInDb.Status = user.Status;
                userInDb.Role = user.Role;
                userInDb.Avatar = user.Avatar;

                // Если задан новый пароль — хешируем и обновляем
                if (!string.IsNullOrWhiteSpace(user.InitialPassword))
                {
                    userInDb.SetPassword(user.InitialPassword);
                }

                // Обновляем в БД
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.InnerException?.Message ?? ex.Message}",
                                "Ошибка БД",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка при обновлении пользователя: {ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
