using CommunityToolkit.Mvvm.ComponentModel;
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

    public partial class User : ObservableValidator
    {
        [Key]
        public int Id { get; set; } // Уникальный идентификатор пользователя

        // Вкладка General
        [ObservableProperty]
        [Required, MaxLength(50)]
        private string firstName; // Имя пользователя

        [ObservableProperty]
        [MaxLength(10)]
        private string initials = string.Empty; // Инициалы пользователя

        [ObservableProperty]
        [Required, MaxLength(50)]
        private string lastName; // Фамилия пользователя

        [ObservableProperty]
        [Required, MaxLength(100)]
        private string displayName = string.Empty; // Отображаемое имя пользователя

        [ObservableProperty]
        [MaxLength(255)]
        private string description = string.Empty; // Описание пользователя

        [ObservableProperty]
        [MaxLength(100)]
        private string office = string.Empty; // Офис в котором работает пользователь

        [ObservableProperty]
        [Required, MaxLength(100), EmailAddress]
        private string email = string.Empty; // Электронная почта пользователя

        [ObservableProperty]
        [MaxLength(255), Url]
        private string webPage = string.Empty; // Сайт пользователя

        [ObservableProperty]
        [Column(TypeName = "date")]
        private DateTime? dateOfBirth; // Дата рождения

        // Вкладка Address
        [ObservableProperty]
        [MaxLength(255)]
        private string street = string.Empty; // Улица

        [ObservableProperty]
        [MaxLength(100)]
        private string city = string.Empty; // Город

        [ObservableProperty]
        [MaxLength(100)]
        private string state = string.Empty; // Штат/Регион

        [ObservableProperty]
        [MaxLength(20)]
        private string postalCode = string.Empty; // Почтовый индекс

        [ObservableProperty]
        [MaxLength(100)]
        private string country = string.Empty; // Страна

        // Вкладка Account
        [ObservableProperty]
        [Required, MaxLength(50)]
        private string userLogonName = string.Empty; // Логин пользователя

        [ObservableProperty]
        [Required, MaxLength(255)]
        private string passwordHash = string.Empty; // Хеш пароля пользователя

        [ObservableProperty]
        [Required, MaxLength(255)]
        private string passwordSalt = string.Empty; // Соль для пароля

        [ObservableProperty]
        //private DateTime accountCreated = DateTime.Now;
        private DateTime accountCreated; // Дата создания учетной записи

        [ObservableProperty]
        private UserStatus status = UserStatus.Active; // Статус активности пользователя

        [ObservableProperty]
        private UserRole role = UserRole.User; // Роль пользователя

        // Вкладка Telephones
        [ObservableProperty]
        [MaxLength(20)]
        private string workPhone = string.Empty; // Номер телефона Рабочий

        [ObservableProperty]
        [MaxLength(20)]
        private string mobilePhone = string.Empty; // Номер телефона Мобильный        

        [ObservableProperty]
        [MaxLength(20)]
        private string iPPhone = string.Empty; // Номер IP-телефона

        // Вкладка Organization
        [ObservableProperty]
        [MaxLength(100)]
        private string jobTitle = string.Empty; // Должность

        [ObservableProperty]
        [MaxLength(100)]
        private string department = string.Empty; // Отдел

        [ObservableProperty]
        [MaxLength(100)]
        private string company = string.Empty; // Компания

        [ObservableProperty]
        [MaxLength(100)]
        private string managerName = string.Empty; // Имя прямого начальника пользователя

        [ObservableProperty]
        [Column(TypeName = "LONGBLOB")]
        private byte[]? avatar; // Фото пользователя

        //[ObservableProperty]
        //[NotMapped] // ВАЖНО!!! Не сохраняем в БД
        //private string? initialPassword;

        private string? _initialPassword;

        [NotMapped] // ВАЖНО!!! Не сохраняем в БД
        public string? InitialPassword
        {
            get => _initialPassword;
            set => SetProperty(ref _initialPassword, value);
        }

        // Навигационные свойства: Связи "многие ко многим"
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        
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