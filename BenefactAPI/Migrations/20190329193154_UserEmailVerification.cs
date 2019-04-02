using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class UserEmailVerification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "email_verified",
                table: "users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "nonce",
                table: "users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email_verified",
                table: "users");

            migrationBuilder.DropColumn(
                name: "nonce",
                table: "users");
        }
    }
}
