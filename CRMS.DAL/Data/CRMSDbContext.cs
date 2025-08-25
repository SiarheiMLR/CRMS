using Microsoft.EntityFrameworkCore;
using CRMS.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.IO;
using Transaction = CRMS.Domain.Entities.Transaction;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Diagnostics;

namespace CRMS.DAL.Data
{
    public class CRMSDbContext : DbContext
    {
        public CRMSDbContext(DbContextOptions<CRMSDbContext> options) : base(options) { }

        // Пустой конструктор нужен для миграций
        public CRMSDbContext() { }

        // Здесь будут определяться DbSet для сущностей (например, Users, Tickets)
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<CustomField> CustomFields { get; set; }
        public DbSet<CustomFieldValue> CustomFieldValues { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Queue> Queues { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<GroupRoleMapping> GroupRoleMappings { get; set; }

        // Модель FAQ
        public DbSet<FaqItem> FaqItems { get; set; }
        public DbSet<FaqCategory> FaqCategories { get; set; }
        public DbSet<FaqVote> FaqVotes { get; set; }
        public DbSet<FaqTag> FaqTags { get; set; }
        public DbSet<FaqItemTag> FaqItemTags { get; set; }
        public DbSet<FaqItemHistory> FaqItemHistories { get; set; }


        /// <summary>
        /// Метод <see cref="OnConfiguring(DbContextOptionsBuilder)"/> используется для настройки параметров подключения к базе данных в Entity Framework Core.
        /// <para>
        /// <strong>Что делает этот метод?</strong>
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <strong>Проверка настройки контекста:</strong> Метод проверяет, был ли уже настроен <see cref="DbContextOptionsBuilder"/> с помощью условия <c>if (!optionsBuilder.IsConfigured)</c>. Это нужно, чтобы избежать повторной настройки, если конфигурация уже была выполнена (например, через конструктор с параметрами).
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <strong>Загрузка конфигурации из файла:</strong> Если конфигурация не была выполнена, метод создает экземпляр <see cref="ConfigurationBuilder"/>, который загружает настройки из файла <c>appsettings.json</c>. Этот файл обычно содержит строку подключения к базе данных и другие параметры.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <strong>Настройка подключения к базе данных:</strong> Метод использует строку подключения из конфигурации (<c>configuration.GetConnectionString("DefaultConnection")</c>) и указывает, что в качестве базы данных используется MySQL (или MariaDB) с конкретной версией сервера (<c>new MySqlServerVersion(new System.Version(10, 4))</c>).
        /// </description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Зачем он нужен?</strong>
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <strong>Гибкость:</strong> Этот метод позволяет настраивать подключение к базе данных в зависимости от среды выполнения (например, разработка, тестирование, продакшн). Например, в <c>appsettings.json</c> можно хранить разные строки подключения для разных сред.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <strong>Миграции:</strong> Если контекст создается без параметров (например, при выполнении миграций), этот метод гарантирует, что подключение к базе данных будет настроено.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <strong>Резервный вариант:</strong> Если контекст не был настроен через конструктор (например, в тестах или при ручном создании экземпляра), этот метод обеспечивает работоспособность.
        /// </description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Пример работы:</strong>
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// Если контекст создается через конструктор с параметрами:
        /// <code>
        /// var options = new DbContextOptionsBuilder&lt;CRMSDbContext&gt;()
        ///     .UseMySql("Server=localhost;Database=crms;User=root;Password=12345;", ServerVersion.AutoDetect("Server=localhost;Database=crms;User=root;Password=12345;"))
        ///     .Options;
        /// var context = new CRMSDbContext(options);
        /// </code>
        /// В этом случае метод <see cref="OnConfiguring(DbContextOptionsBuilder)"/> не будет вызван, так как конфигурация уже выполнена.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Если контекст создается без параметров:
        /// <code>
        /// var context = new CRMSDbContext();
        /// </code>
        /// В этом случае метод <see cref="OnConfiguring(DbContextOptionsBuilder)"/> будет вызван, и настройки подключения будут загружены из <c>appsettings.json</c>.
        /// </description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                optionsBuilder.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(10, 4)), // MariaDB 10.4
                    options =>
                    {
                        // Включаем поддержку примитивных коллекций для EF Core 8+
                        options.EnablePrimitiveCollectionsSupport();

                        // Дополнительные оптимизации
                        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        options.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });

                // Для отладки включаем детализированное логирование
                optionsBuilder.LogTo(message => Debug.WriteLine(message))
                             .EnableSensitiveDataLogging()
                             .EnableDetailedErrors();

                // Игнорируем предупреждение об изменениях модели
                optionsBuilder.ConfigureWarnings(warnings => warnings
                    .Ignore(RelationalEventId.PendingModelChangesWarning));
            }
        }

        //// Старый вариант метода
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        var configuration = new ConfigurationBuilder()
        //            .SetBasePath(Directory.GetCurrentDirectory())
        //            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //            .Build();

        //        optionsBuilder.UseMySql(configuration.GetConnectionString("DefaultConnection"),
        //            new MySqlServerVersion(new System.Version(10, 4))); // MariaDB 10.4
        //        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        //    }
        //}

        ///-----Это альтернативный способ настройки, который может быть полезен для быстрого тестирования
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseMySql(
        //            "Server=localhost;Database=crms;User=Administrator;Password=27011984Hp;",
        //            ServerVersion.AutoDetect("Server=localhost;Database=crms;User=Administrator;Password=27011984Hp;"));
        //    }
        //}

        /// <summary>
        /// Метод <see cref="OnModelCreating(ModelBuilder)"/> в Entity Framework Core используется для дополнительной настройки модели базы данных.
        /// Этот метод позволяет вручную настроить отношения между сущностями, индексы, ограничения и другие аспекты модели, которые не могут быть автоматически определены EF Core на основе классов сущностей.
        /// <para>
        /// <strong>Что делает метод <see cref="OnModelCreating(ModelBuilder)"/>?</strong>
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <strong>Настройка уникального индекса:</strong> В коде метод настраивает уникальный индекс для свойства <c>Email</c> в сущности <c>User</c>:
        /// <code>
        /// modelBuilder.Entity&lt;User&gt;().HasIndex(u => u.Email).IsUnique();
        /// </code>
        /// Это означает, что в таблице <c>Users</c> будет создан уникальный индекс для столбца <c>Email</c>, чтобы гарантировать, что два пользователя не могут иметь одинаковый email.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <strong>Настройка отношений между сущностями:</strong> Метод настраивает отношения между сущностями <c>GroupMember</c>, <c>User</c> и <c>Group</c>:
        /// <code>
        /// modelBuilder.Entity&lt;GroupMember&gt;()
        ///     .HasOne(gm => gm.User)
        ///     .WithMany(u => u.GroupMembers)
        ///     .HasForeignKey(gm => gm.UserId)
        ///     .OnDelete(DeleteBehavior.Cascade);
        ///
        /// modelBuilder.Entity&lt;GroupMember&gt;()
        ///     .HasOne(gm => gm.Group)
        ///     .WithMany(g => g.GroupMembers)
        ///     .HasForeignKey(gm => gm.GroupId)
        ///     .OnDelete(DeleteBehavior.Cascade);
        /// </code>
        /// Здесь:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <c>GroupMember</c> имеет связь "один ко многим" с <c>User</c> (один пользователь может быть членом нескольких групп).
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <c>GroupMember</c> также имеет связь "один ко многим" с <c>Group</c> (одна группа может содержать несколько участников).
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Указывается, что при удалении связанной сущности (<c>User</c> или <c>Group</c>) все связанные записи в <c>GroupMember</c> будут удалены каскадно (<c>DeleteBehavior.Cascade</c>).
        /// </description>
        /// </item>
        /// </list>
        /// </description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Зачем это нужно?</strong>
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <strong>Уникальные индексы:</strong> Гарантируют целостность данных, предотвращая дублирование значений в определенных столбцах (например, email пользователя).
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <strong>Настройка отношений:</strong> Позволяет явно указать, как сущности связаны между собой, и как они должны вести себя при удалении или обновлении связанных данных.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <strong>Дополнительные настройки:</strong> В этом методе можно настроить множество других аспектов модели, таких как:
        /// <list type="bullet">
        /// <item>
        /// <description>Составные ключи.</description>
        /// </item>
        /// <item>
        /// <description>Ограничения на уровне базы данных.</description>
        /// </item>
        /// <item>
        /// <description>Настройка наследования (например, TPH, TPT, TPC).</description>
        /// </item>
        /// <item>
        /// <description>Настройка преобразований данных (например, хранение enum как строки).</description>
        /// </item>
        /// </list>
        /// </description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Пример работы:</strong>
        /// Предположим, у тебя есть следующие сущности:
        /// <code>
        /// public class User
        /// {
        ///     public int Id { get; set; }
        ///     public string Email { get; set; }
        ///     public List&lt;GroupMember&gt; GroupMembers { get; set; }
        /// }
        ///
        /// public class Group
        /// {
        ///     public int Id { get; set; }
        ///     public string Name { get; set; }
        ///     public List&lt;GroupMember&gt; GroupMembers { get; set; }
        /// }
        ///
        /// public class GroupMember
        /// {
        ///     public int UserId { get; set; }
        ///     public User User { get; set; }
        ///
        ///     public int GroupId { get; set; }
        ///     public Group Group { get; set; }
        /// }
        /// </code>
        /// Благодаря методу <see cref="OnModelCreating(ModelBuilder)"/>:
        /// <list type="bullet">
        /// <item>
        /// <description>В таблице <c>Users</c> будет уникальный индекс для столбца <c>Email</c>.</description>
        /// </item>
        /// <item>
        /// <description>В таблице <c>GroupMembers</c> будут внешние ключи <c>UserId</c> и <c>GroupId</c>, которые ссылаются на таблицы <c>Users</c> и <c>Groups</c> соответственно.</description>
        /// </item>
        /// <item>
        /// <description>При удалении пользователя или группы все связанные записи в <c>GroupMembers</c> будут автоматически удалены.</description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Итог:</strong>
        /// Метод <see cref="OnModelCreating(ModelBuilder)"/> — это мощный инструмент для тонкой настройки модели базы данных в Entity Framework Core. Он позволяет явно указать, как сущности и их отношения должны быть отображены в базе данных, что особенно полезно в сложных сценариях, где автоматических настроек EF Core недостаточно.
        /// </para>
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>(entity =>
            {
                modelBuilder.Entity<Ticket>()
                    .HasOne(t => t.Requestor)
                    .WithMany() // Нет обратной навигации
                    .HasForeignKey(t => t.RequestorId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<Ticket>()
                    .HasOne(t => t.Supporter)
                    .WithMany() // Нет обратной навигации
                    .HasForeignKey(t => t.SupporterId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                modelBuilder.Entity<Ticket>()
                            .HasMany(t => t.Attachments)
                            .WithOne(a => a.Ticket)
                            .HasForeignKey(a => a.TicketId)
                            .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<Ticket>()
                            .Property(t => t.Priority)
                            .HasConversion<int>(); // Явное указание конвертации в int

                modelBuilder.Entity<Group>()
                    .HasOne(g => g.GroupRoleMapping)
                    .WithOne(m => m.Group)
                    .HasForeignKey<GroupRoleMapping>(grm => grm.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

            });

            // Конфигурация Attachment
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.ToTable("Attachments");

                entity.HasKey(a => a.Id);

                entity.Property(a => a.FileName)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(a => a.ContentType)
                      .HasMaxLength(100);

                entity.Property(a => a.FileData)
                      .HasColumnType("LONGBLOB") // Для MySQL
                      .IsRequired();

                entity.Property(a => a.FileSize);

                entity.Property(a => a.UploadedAt)
                      .HasDefaultValueSql("UTC_TIMESTAMP()");

                // Связь с Ticket
                entity.HasOne(a => a.Ticket)
                      .WithMany(t => t.Attachments)
                      .HasForeignKey(a => a.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Связь с User (кто загрузил)
                entity.HasOne(a => a.UploadedBy)
                      .WithMany()
                      .HasForeignKey(a => a.UploadedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Создание или обновление в базе дынных учетной записи Administrator
            var adminEmail = "admin@bigfirm.by";
            var adminUserName = "administrator@bigfirm.by";
            var adminPassword = "27011984Hp";

            var salt = User.GenerateSalt();
            var hash = User.HashPassword(adminPassword, salt);

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FirstName = "Siarhei",
                Initials = "Y",
                LastName = "Kuzmich",
                DisplayName = "Siarhei Kuzmich",
                Description = "Встроенная учетная запись для управления системой CRMS и доменом bigfirm.by",
                Office = "не указан",
                Email = adminEmail,
                WebPage = "http://www.bigfirm.by",
                DateOfBirth = new DateTime(1984, 1, 27),
                Street = "Sovetskay 113",
                City = "Malorita",
                State = "Brest region",
                PostalCode = "225903",
                Country = "Belarus",
                UserLogonName = adminUserName,
                WorkPhone = "+375-33-6575238",
                MobilePhone = "+375-29-7012884",
                IPPhone = "не указан",
                JobTitle = "System Administrator",
                Department = "IT Support",
                Company = "BIGFIRM",
                ManagerName = "не указан",
                PasswordHash = hash,
                PasswordSalt = salt,
                AccountCreated = DateTime.UtcNow,
                Status = UserStatus.Active,
                Role = UserRole.Admin
            });
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            // Явно исключаем InitialPassword
            modelBuilder.Entity<User>()
                .Ignore(u => u.InitialPassword);
            modelBuilder.Entity<GroupMember>()
               .HasOne(gm => gm.User)
               .WithMany(u => u.GroupMembers)
               .HasForeignKey(gm => gm.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.GroupMembers)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<GroupRoleMapping>()
            //    .HasOne(m => m.Group)
            //    .WithMany()
            //    .HasForeignKey(m => m.GroupId)
            //    .OnDelete(DeleteBehavior.Cascade);

            // Модель FAQ
            modelBuilder.Entity<FaqItemTag>()
                .HasKey(t => new { t.FaqItemId, t.FaqTagId });

            modelBuilder.Entity<FaqItemTag>()
                .HasOne(ft => ft.FaqItem)
                .WithMany(fi => fi.FaqItemTags)
                .HasForeignKey(ft => ft.FaqItemId);

            modelBuilder.Entity<FaqItemTag>()
                .HasOne(ft => ft.FaqTag)
                .WithMany(t => t.FaqItemTags)
                .HasForeignKey(ft => ft.FaqTagId);

            modelBuilder.Entity<Queue>(b =>
            {
                b.Property(q => q.SlaResponseTime).HasColumnType("time");
                b.Property(q => q.SlaResolutionTime).HasColumnType("time");

                b.HasIndex(q => q.CorrespondAddress);
                b.HasIndex(q => q.CommentAddress);
            });
        }

        public void EnsureAdminUser()
        {
            var adminEmail = "admin@bigfirm.by";
            var existingAdmin = Users.FirstOrDefault(u => u.Email == adminEmail);

            if (existingAdmin == null)
            {
                // Создаём учетную запись администратора, если его нет
                var salt = User.GenerateSalt();
                var hash = User.HashPassword("27011984Hp", salt);

                var newAdmin = new User
                {
                    FirstName = "Siarhei",
                    Initials = "Y",
                    LastName = "Kuzmich",
                    DisplayName = "Siarhei Kuzmich",
                    Description = "Встроенная учетная запись для управления системой CRMS и доменом bigfirm.by",
                    Office = "не указан",
                    Email = adminEmail,
                    WebPage = "http://www.bigfirm.by",
                    DateOfBirth = new DateTime(1984, 1, 27),
                    Street = "Sovetskay 113",
                    City = "Malorita",
                    State = "Brest region",
                    PostalCode = "225903",
                    Country = "Belarus",
                    UserLogonName = "administrator@bigfirm.by",
                    WorkPhone = "+375-33-6575238",
                    MobilePhone = "+375-29-7012884",
                    IPPhone = "не указан",
                    JobTitle = "System Administrator",
                    Department = "IT Support",
                    Company = "BIGFIRM",
                    ManagerName = "не указан",
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    AccountCreated = DateTime.UtcNow,
                    Status = UserStatus.Active,
                    Role = UserRole.Admin
                };
                Users.Add(newAdmin);
            }
            else
            {
                // Если учетная запись администратора уже есть, обновляем ее данные
                existingAdmin.FirstName = "Siarhei";
                existingAdmin.Initials = "S.K.";
                existingAdmin.LastName = "Kuzmich";
                existingAdmin.DisplayName = "Siarhei Kuzmich";
                existingAdmin.Description = "Встроенная учетная запись для управления системой CRMS и доменом bigfirm.by";
                existingAdmin.Office = "не указан";
                existingAdmin.Email = adminEmail;
                existingAdmin.WebPage = "http://www.bigfirm.by";
                existingAdmin.DateOfBirth = new DateTime(1984, 1, 27);
                existingAdmin.Street = "Sovetskay 113";
                existingAdmin.City = "Malorita";
                existingAdmin.State = "Brest region";
                existingAdmin.PostalCode = "225903";
                existingAdmin.Country = "Belarus";
                existingAdmin.UserLogonName = "administrator@bigfirm.by";
                existingAdmin.WorkPhone = "+375-33-6575238";
                existingAdmin.MobilePhone = "+375-29-7012884";
                existingAdmin.IPPhone = "не указан";
                existingAdmin.JobTitle = "System Administrator";
                existingAdmin.Department = "IT Support";
                existingAdmin.Company = "BIGFIRM";
                existingAdmin.ManagerName = "не указан";
                //existingAdmin.PasswordHash = hash;
                //existingAdmin.PasswordSalt = salt;
                existingAdmin.AccountCreated = DateTime.UtcNow;
                existingAdmin.Status = UserStatus.Active;
                existingAdmin.Role = UserRole.Admin;                
            }

            SaveChanges();
        }
    }
}
