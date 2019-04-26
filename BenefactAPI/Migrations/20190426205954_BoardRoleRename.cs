using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class BoardRoleRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_board_role_boards_board_id",
                table: "board_role");

            migrationBuilder.DropForeignKey(
                name: "fk_user_board_role_board_role_board_role_board_id_board_role_id",
                table: "user_board_role");

            migrationBuilder.DropPrimaryKey(
                name: "pk_board_role",
                table: "board_role");

            migrationBuilder.RenameTable(
                name: "board_role",
                newName: "roles");

            migrationBuilder.AddPrimaryKey(
                name: "pk_roles",
                table: "roles",
                columns: new[] { "board_id", "id" });

            migrationBuilder.AddForeignKey(
                name: "fk_roles_boards_board_id",
                table: "roles",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_board_role_roles_board_role_board_id_board_role_id",
                table: "user_board_role",
                columns: new[] { "board_role_board_id", "board_role_id" },
                principalTable: "roles",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_roles_boards_board_id",
                table: "roles");

            migrationBuilder.DropForeignKey(
                name: "fk_user_board_role_roles_board_role_board_id_board_role_id",
                table: "user_board_role");

            migrationBuilder.DropPrimaryKey(
                name: "pk_roles",
                table: "roles");

            migrationBuilder.RenameTable(
                name: "roles",
                newName: "board_role");

            migrationBuilder.AddPrimaryKey(
                name: "pk_board_role",
                table: "board_role",
                columns: new[] { "board_id", "id" });

            migrationBuilder.AddForeignKey(
                name: "fk_board_role_boards_board_id",
                table: "board_role",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_board_role_board_role_board_role_board_id_board_role_id",
                table: "user_board_role",
                columns: new[] { "board_role_board_id", "board_role_id" },
                principalTable: "board_role",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
