using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class BoardNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "boards",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "url_name",
                table: "boards",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("update boards set url_name = trim(to_char(id, '99999'));");

            migrationBuilder.CreateIndex(
                name: "ix_boards_url_name",
                table: "boards",
                column: "url_name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_boards_url_name",
                table: "boards");

            migrationBuilder.DropColumn(
                name: "title",
                table: "boards");

            migrationBuilder.DropColumn(
                name: "url_name",
                table: "boards");
        }
    }
}
