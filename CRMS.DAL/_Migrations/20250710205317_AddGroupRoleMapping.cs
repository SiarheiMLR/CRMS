using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupRoleMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Groups_GroupId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_GroupId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "GroupRoleMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRoleMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupRoleMappings_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountCreated", "PasswordHash", "PasswordSalt" },
                values: new object[] { new DateTime(2025, 7, 10, 20, 53, 14, 957, DateTimeKind.Utc).AddTicks(6967), "q70Neq0YQkZwIt33ebbunH1eCGNwHGWhSG+HlaeiU5o=", "mfrd8ryYpqAuU03r6V2f2g==" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoleMappings_GroupId",
                table: "GroupRoleMappings",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupRoleMappings");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountCreated", "GroupId", "PasswordHash", "PasswordSalt" },
                values: new object[] { new DateTime(2025, 7, 10, 7, 31, 22, 809, DateTimeKind.Utc).AddTicks(4584), null, "w785I9nb7QoOxWX3GnbDKD+iv77/sEQceq64Hyy5ZHM=", "oP7dJz7yqzGlCZ+Xt0RlcA==" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_GroupId",
                table: "Users",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Groups_GroupId",
                table: "Users",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
