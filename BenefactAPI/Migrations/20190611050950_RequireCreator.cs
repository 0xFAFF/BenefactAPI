using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class RequireCreator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "creator_id",
                table: "boards",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "creator_id",
                table: "boards",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
