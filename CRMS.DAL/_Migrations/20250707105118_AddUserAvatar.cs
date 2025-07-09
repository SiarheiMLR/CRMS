using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Avatar",
                table: "Users",
                type: "LONGBLOB",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountCreated", "Avatar", "PasswordHash", "PasswordSalt" },
                values: new object[] { new DateTime(2025, 7, 7, 10, 51, 15, 667, DateTimeKind.Utc).AddTicks(4205), null, "jtEYDgFQK0GNnVp3PINx7wnkqZxxSVmQKjjv522GeNU=", "UokqvMbrUkinUtwi6dMnOA==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountCreated", "PasswordHash", "PasswordSalt" },
                values: new object[] { new DateTime(2025, 3, 7, 20, 9, 52, 528, DateTimeKind.Utc).AddTicks(4836), "CHGEbLVVd2o7UGw4XPcoeLsOiAIm9kwhVs5ljYiLbts=", "ZOoZibOqtPLaz8MCOw8EtA==" });
        }
    }
}
