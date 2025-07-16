using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixGroupRoleRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // НЕ ТРОГАЕМ "FK_Groups_GroupRoleMappings_GroupId"

            migrationBuilder.DropForeignKey(
                name: "FK_GroupRoleMappings_Groups_GroupId",
                table: "GroupRoleMappings");

            migrationBuilder.DropIndex(
                name: "IX_GroupRoleMappings_GroupId",
                table: "GroupRoleMappings");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoleMappings_GroupId",
                table: "GroupRoleMappings",
                column: "GroupId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRoleMappings_Groups_GroupId",
                table: "GroupRoleMappings",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }



        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Удалить FK перед удалением индекса
            migrationBuilder.DropForeignKey(
                name: "FK_GroupRoleMappings_Groups_GroupId",
                table: "GroupRoleMappings");

            migrationBuilder.DropIndex(
                name: "IX_GroupRoleMappings_GroupId",
                table: "GroupRoleMappings");

            // Восстановить старую версию индекса (если нужно)
            migrationBuilder.CreateIndex(
                name: "IX_GroupRoleMappings_GroupId",
                table: "GroupRoleMappings",
                column: "GroupId");

            // Восстановить данные
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountCreated", "PasswordHash", "PasswordSalt" },
                values: new object[]
                {
            new DateTime(2025, 7, 10, 20, 53, 14, 957, DateTimeKind.Utc).AddTicks(6967),
            "q70Neq0YQkZwIt33ebbunH1eCGNwHGWhSG+HlaeiU5o=",
            "mfrd8ryYpqAuU03r6V2f2g=="
                });
        }

    }
}
