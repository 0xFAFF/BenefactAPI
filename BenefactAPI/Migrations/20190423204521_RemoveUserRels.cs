using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class RemoveUserRels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_board_role_boards_id",
                table: "board_role");

            migrationBuilder.DropIndex(
                name: "ix_board_role_id",
                table: "board_role");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_board_role_id",
                table: "board_role",
                column: "id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_board_role_boards_id",
                table: "board_role",
                column: "id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
