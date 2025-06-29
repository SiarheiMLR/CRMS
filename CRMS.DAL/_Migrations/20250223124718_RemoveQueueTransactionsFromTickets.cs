using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQueueTransactionsFromTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Queues_QueueId",
                table: "Tickets");

            migrationBuilder.AlterColumn<int>(
                name: "QueueId",
                table: "Tickets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountCreated", "PasswordHash", "PasswordSalt" },
                values: new object[] { new DateTime(2025, 2, 23, 12, 47, 16, 835, DateTimeKind.Utc).AddTicks(1157), "S3lDndKbrIQc4SB9uWrCr/LuEVjgI8t8+MHGXq6qC+k=", "h1v5hWj7CgeIW6+qO1AX4A==" });

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Queues_QueueId",
                table: "Tickets",
                column: "QueueId",
                principalTable: "Queues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Queues_QueueId",
                table: "Tickets");

            migrationBuilder.AlterColumn<int>(
                name: "QueueId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountCreated", "PasswordHash", "PasswordSalt" },
                values: new object[] { new DateTime(2025, 2, 22, 15, 28, 59, 19, DateTimeKind.Utc).AddTicks(9691), "FLIaCpWNIMBNfCVt4bZV5q2wurEDqZMvxKf8N0we4yE=", "QwDj1nB10JFZG17ELMJRDQ==" });

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Queues_QueueId",
                table: "Tickets",
                column: "QueueId",
                principalTable: "Queues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
