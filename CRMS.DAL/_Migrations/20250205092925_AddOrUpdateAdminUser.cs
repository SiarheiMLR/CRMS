using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddOrUpdateAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccountCreated", "City", "Company", "Country", "DateOfBirth", "Department", "Description", "DisplayName", "Email", "FirstName", "IPPhone", "Initials", "JobTitle", "LastName", "ManagerName", "MobilePhone", "Office", "PasswordHash", "PasswordSalt", "PostalCode", "Role", "State", "Status", "Street", "UserLogonName", "WebPage", "WorkPhone" },
                values: new object[] { 1, new DateTime(2025, 2, 5, 9, 29, 22, 183, DateTimeKind.Utc).AddTicks(5266), "Malorita", "BIGFIRM", "Belarus", new DateTime(1984, 1, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "IT Support", "Встроенная учетная запись для управления системой CRMS и доменом bigfirm.by", "Siarhei Kuzmich", "admin@bigfirm.by", "Siarhei", "не указан", "Y", "System Administrator", "Kuzmich", "не указан", "+375-29-7012884", "не указан", "NvvjmRISlIfiawweQSy/WkfQhPBsYi9+gbA3P9uF+ZA=", "Pl+uHAiD+8wWFRzAhU38/A==", "225903", 2, "Brest region", 0, "Sovetskay 113", "administrator@bigfirm.by", "http://www.bigfirm.by", "+375-33-6575238" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
