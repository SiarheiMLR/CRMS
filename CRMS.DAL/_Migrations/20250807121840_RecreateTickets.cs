using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RecreateTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TicketId",
                table: "CustomFieldValues",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountCreated", "PasswordHash", "PasswordSalt" },
                values: new object[] { new DateTime(2025, 8, 7, 12, 18, 37, 21, DateTimeKind.Utc).AddTicks(5291), "4aoC56+AmYj/V7Od+IMZ+gjq64H9y7xbS4QjBC3xXwo=", "724nSn7CvzSaacaBTHWlvQ==" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldValues_TicketId",
                table: "CustomFieldValues",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomFieldValues_Tickets_TicketId",
                table: "CustomFieldValues",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomFieldValues_Tickets_TicketId",
                table: "CustomFieldValues");

            migrationBuilder.DropIndex(
                name: "IX_CustomFieldValues_TicketId",
                table: "CustomFieldValues");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "CustomFieldValues");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountCreated", "PasswordHash", "PasswordSalt" },
                values: new object[] { new DateTime(2025, 8, 3, 13, 54, 10, 101, DateTimeKind.Utc).AddTicks(5797), "ovC+Imm7I3EhomLVEPaSz5cA741janBb90iTSPa3Ro8=", "iWBGh2/zJ+I7rIyDIvWZXA==" });
        }
    }
}
