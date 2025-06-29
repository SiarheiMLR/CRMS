using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace CRMS.Domain.Entities
{
    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended
    }

    public enum UserRole
    {
        User,
        Support,
        Admin
    }

    public class User
    {
        [Key]
        public int Id { get; set; } // Уникальный идентификатор пользователя

        // Вкладка General
        [Required, MaxLength(50)]
        public string FirstName { get; set; } // Имя пользователя

        [MaxLength(10)]
        public string Initials { get; set; } = string.Empty; // Инициалы пользователя

        [Required, MaxLength(50)]
        public string LastName { get; set; } // Фамилия пользователя

        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty; // Отображаемое имя пользователя

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty; // Описание пользователя

        [MaxLength(100)]
        public string Office { get; set; } = string.Empty; // Офис в котором работает пользователь

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; } = string.Empty; // Электронная почта пользователя

        [MaxLength(255), Url]
        public string WebPage { get; set; } = string.Empty; // Сайт пользователя

        [Column(TypeName = "date")]
        public DateTime? DateOfBirth { get; set; } // Дата рождения

        // Вкладка Address
        [MaxLength(255)]
        public string Street { get; set; } = string.Empty; // Улица

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;// Город

        [MaxLength(100)]
        public string State { get; set; } = string.Empty;// Штат/Регион

        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;// Почтовый индекс

        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;// Страна

        // Вкладка Account
        [Required, MaxLength(50)]
        public string UserLogonName { get; set; } = string.Empty; // Логин пользователя

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty; // Хеш пароля пользователя

        [Required, MaxLength(255)]
        public string PasswordSalt { get; set; } = string.Empty; // Соль для пароля

        public DateTime AccountCreated { get; set; } = DateTime.UtcNow; // Дата создания учетной записи

        public UserStatus Status { get; set; } = UserStatus.Active; // Статус активности пользователя

        public UserRole Role { get; set; } = UserRole.User; // Роль пользователя

        // Вкладка Telephones
        [MaxLength(20)]
        public string WorkPhone { get; set; } = string.Empty;// Номер телефона Рабочий

        [MaxLength(20)]
        public string MobilePhone { get; set; } = string.Empty;// Номер телефона Мобильный        

        [MaxLength(20)]
        public string IPPhone { get; set; } = string.Empty;// Номер IP-телефона

        // Вкладка Organization
        [MaxLength(100)]
        public string JobTitle { get; set; } = string.Empty;// Должность

        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;// Отдел

        [MaxLength(100)]
        public string Company { get; set; } = string.Empty;// Компания

        [MaxLength(100)]
        public string ManagerName { get; set; } = string.Empty;// Имя прямого начальника пользователя

        // Связи "многие ко многим"
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Group> Groups { get; set; } = new List<Group>();
        public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

        // Методы для работы с паролем
        public void SetPassword(string password)
        {
            PasswordSalt = GenerateSalt();
            PasswordHash = HashPassword(password, PasswordSalt);
        }

        public bool ValidatePassword(string password)
        {
            return PasswordHash == HashPassword(password, PasswordSalt);
        }

        /// <summary>
        /// ----Использовался устаревший RNGCryptoServiceProvider вместо RandomNumberGenerator
        /// </summary>
        /// <returns></returns>
        //private string GenerateSalt()
        //{
        //    using (var rng = new RNGCryptoServiceProvider())
        //    {
        //        byte[] saltBytes = new byte[16];
        //        rng.GetBytes(saltBytes);
        //        return Convert.ToBase64String(saltBytes);
        //    }
        //}
        public static string GenerateSalt()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] saltBytes = new byte[16];
                rng.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }
        }

        public static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }    
}