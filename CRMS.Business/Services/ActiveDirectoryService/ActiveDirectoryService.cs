using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CRMS.Business.Models;

namespace CRMS.Business.ActiveDirectoryService
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        /// <summary>
        /// Получает всех пользователей из указанного домена.
        /// </summary>
        /// <param name="domain">Домен, например "bigfirm.by"</param>
        /// <param name="login">Логин администратора</param>
        /// <param name="password">Пароль администратора</param>
        public async Task<List<ADUserDto>> GetAllUsersAsync(string domain, string login, string password)
        {
            // Оборачиваем синхронный LDAP-запрос в Task.Run для предотвращения блокировки UI-потока
            return await Task.Run(() =>
            {
                var result = new List<ADUserDto>();

                try
                {
                    using var context = new PrincipalContext(ContextType.Domain, domain, login, password);
                    using var searcher = new PrincipalSearcher(new UserPrincipal(context));

                    foreach (var principal in searcher.FindAll().OfType<UserPrincipal>())
                    {
                        if (principal.SamAccountName == null)
                            continue;

                        var entry = (DirectoryEntry)principal.GetUnderlyingObject();

                        var user = new ADUserDto
                        {
                            FirstName = principal.GivenName ?? "Не указано",
                            LastName = principal.Surname ?? "Не указано",
                            Initials = Get(entry, "initials"),
                            DisplayName = principal.DisplayName ?? "Не указано",
                            Description = Get(entry, "description"),
                            Office = Get(entry, "physicalDeliveryOfficeName"),
                            Email = principal.EmailAddress ?? "Не указано",
                            WebPage = Get(entry, "wWWHomePage"),
                            DateOfBirth = ParseDate(entry, "birthDate"),
                            Street = Get(entry, "streetAddress"),
                            City = Get(entry, "l"),
                            State = Get(entry, "st"),
                            PostalCode = Get(entry, "postalCode"),
                            Country = Get(entry, "co"),
                            UserLogonName = $"{principal.SamAccountName}@{domain}",
                            WorkPhone = Get(entry, "telephoneNumber"),
                            MobilePhone = Get(entry, "mobile"),
                            IPPhone = Get(entry, "ipPhone"),
                            JobTitle = Get(entry, "title"),
                            Department = Get(entry, "department"),
                            Company = Get(entry, "company"),
                            ManagerName = Get(entry, "manager")
                        };

                        result.Add(user);
                    }

                    return result.OrderBy(u => u.DisplayName).ToList();
                }
                catch (COMException ex) when ((uint)ex.ErrorCode == 0x8007052E) // Неверный логин/пароль
                {
                    throw new UnauthorizedAccessException("Неверный логин или пароль администратора домена.", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка при получении пользователей из Active Directory.", ex);
                }
            });
        }

        private string Get(DirectoryEntry entry, string propName)
        {
            return entry.Properties.Contains(propName)
                ? entry.Properties[propName]?.Value?.ToString() ?? "Не указано"
                : "Не указано";
        }

        private DateTime? ParseDate(DirectoryEntry entry, string propName)
        {
            if (entry.Properties.Contains(propName))
            {
                var raw = entry.Properties[propName]?.Value?.ToString();
                if (DateTime.TryParse(raw, out var date))
                    return date;
            }
            return null;
        }
    }
}
