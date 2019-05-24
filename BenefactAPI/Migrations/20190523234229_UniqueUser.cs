using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class UniqueUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "users",
                type: "varchar(64)",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "varchar(128)",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.DropUniqueConstraint(
                name: "ak_users_email",
                table: "users");
            // NOTE REMEMBER: These are MANUAL
            migrationBuilder.Sql("create unique index ak_users_name on users (lower(name))");
            migrationBuilder.Sql("create unique index ak_users_email on users (lower(email))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop index ak_users_name");
            migrationBuilder.Sql("drop index ak_users_email");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_users_email",
                table: "users",
                column: "email");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "users",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(128)");
        }
    }
}
