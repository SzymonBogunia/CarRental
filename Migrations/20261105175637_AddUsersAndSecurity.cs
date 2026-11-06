using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Login = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "INTEGER", nullable: false),
                    OneTimePasswordExpiresUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PasswordChangedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsBlocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    LastActivityUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecuritySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    MinPasswordLength = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonalDataRuleEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordValidityDays = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionIdleMinutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecuritySettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordHistory_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Login",
                table: "AppUsers",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistory_AppUserId",
                table: "PasswordHistory",
                column: "AppUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordHistory");

            migrationBuilder.DropTable(
                name: "SecuritySettings");

            migrationBuilder.DropTable(
                name: "AppUsers");
        }
    }
}
