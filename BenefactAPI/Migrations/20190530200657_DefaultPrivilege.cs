using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class DefaultPrivilege : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "boards",
                column: "default_privilege",
                keyColumn: "default_privilege",
                keyValue: null,
                value: 0);
            migrationBuilder.AlterColumn<int>(
                name: "default_privilege",
                table: "boards",
                defaultValue: 0,
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "default_privilege",
                table: "boards",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
