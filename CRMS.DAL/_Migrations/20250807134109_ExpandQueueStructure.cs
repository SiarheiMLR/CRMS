using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ExpandQueueStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentQueueId",
                table: "Queues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SlaResolutionTime",
                table: "Queues",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SlaResponseTime",
                table: "Queues",
                type: "time(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QueueAutomationRule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QueueId = table.Column<int>(type: "int", nullable: false),
                    TriggerEvent = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Condition = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueAutomationRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueueAutomationRule_Queues_QueueId",
                        column: x => x.QueueId,
                        principalTable: "Queues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QueuePermission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QueueId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Permission = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuePermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueuePermission_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueuePermission_Queues_QueueId",
                        column: x => x.QueueId,
                        principalTable: "Queues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketStatusDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QueueId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketStatusDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketStatusDefinition_Queues_QueueId",
                        column: x => x.QueueId,
                        principalTable: "Queues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountCreated", "PasswordHash", "PasswordSalt" },
                values: new object[] { new DateTime(2025, 8, 7, 13, 41, 6, 659, DateTimeKind.Utc).AddTicks(2412), "BakKGnGaKjiRVKVnlApPodR0kxQrltKzbGI4UIMG9vQ=", "5YNt82J0tC9y2cW9Oy9CHA==" });

            migrationBuilder.CreateIndex(
                name: "IX_Queues_ParentQueueId",
                table: "Queues",
                column: "ParentQueueId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueAutomationRule_QueueId",
                table: "QueueAutomationRule",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_QueuePermission_GroupId",
                table: "QueuePermission",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_QueuePermission_QueueId",
                table: "QueuePermission",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusDefinition_QueueId",
                table: "TicketStatusDefinition",
                column: "QueueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Queues_Queues_ParentQueueId",
                table: "Queues",
                column: "ParentQueueId",
                principalTable: "Queues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Queues_Queues_ParentQueueId",
                table: "Queues");

            migrationBuilder.DropTable(
                name: "QueueAutomationRule");

            migrationBuilder.DropTable(
                name: "QueuePermission");

            migrationBuilder.DropTable(
                name: "TicketStatusDefinition");

            migrationBuilder.DropIndex(
                name: "IX_Queues_ParentQueueId",
                table: "Queues");

            migrationBuilder.DropColumn(
                name: "ParentQueueId",
                table: "Queues");

            migrationBuilder.DropColumn(
                name: "SlaResolutionTime",
                table: "Queues");

            migrationBuilder.DropColumn(
                name: "SlaResponseTime",
                table: "Queues");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountCreated", "PasswordHash", "PasswordSalt" },
                values: new object[] { new DateTime(2025, 8, 7, 12, 18, 37, 21, DateTimeKind.Utc).AddTicks(5291), "4aoC56+AmYj/V7Od+IMZ+gjq64H9y7xbS4QjBC3xXwo=", "724nSn7CvzSaacaBTHWlvQ==" });
        }
    }
}
