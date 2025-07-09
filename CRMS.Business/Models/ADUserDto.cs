using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Business.Models
{
    public class ADUserDto
    {
        public string FirstName { get; set; } = "Не указано";
        public string Initials { get; set; } = "Не указано";
        public string LastName { get; set; } = "Не указано";
        public string DisplayName { get; set; } = "Не указано";
        public string Description { get; set; } = "Не указано";
        public string Office { get; set; } = "Не указано";
        public string Email { get; set; } = "Не указано";
        public string WebPage { get; set; } = "Не указано";
        public DateTime? DateOfBirth { get; set; } = null; // ✔️ исправлено
        public string Street { get; set; } = "Не указано";
        public string City { get; set; } = "Не указано";
        public string State { get; set; } = "Не указано";
        public string PostalCode { get; set; } = "Не указано";
        public string Country { get; set; } = "Не указано";
        public string UserLogonName { get; set; } = "Не указано";
        public byte[]? Avatar { get; set; }
        public string WorkPhone { get; set; } = "Не указано";
        public string MobilePhone { get; set; } = "Не указано";
        public string IPPhone { get; set; } = "Не указано";
        public string JobTitle { get; set; } = "Не указано";
        public string Department { get; set; } = "Не указано";
        public string Company { get; set; } = "Не указано";
        public string ManagerName { get; set; } = "Не указано";
    }
}

