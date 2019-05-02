using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class BoardRoleKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_board_role_roles_board_role_board_id_board_role_id",
                table: "user_board_role");

            migrationBuilder.DropIndex(
                name: "ix_user_board_role_board_id",
                table: "user_board_role");

            migrationBuilder.DropIndex(
                name: "ix_user_board_role_board_role_board_id_board_role_id",
                table: "user_board_role");

            migrationBuilder.DropColumn(
                name: "board_role_board_id",
                table: "user_board_role");

            migrationBuilder.AlterColumn<int>(
                name: "board_role_id",
                table: "user_board_role",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ik_board_role",
                table: "user_board_role",
                columns: new[] { "board_id", "board_role_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_board_role",
                table: "user_board_role",
                columns: new[] { "board_id", "board_role_id" },
                principalTable: "roles",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_board_role",
                table: "user_board_role");

            migrationBuilder.DropIndex(
                name: "ik_board_role",
                table: "user_board_role");

            migrationBuilder.AlterColumn<int>(
                name: "board_role_id",
                table: "user_board_role",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "board_role_board_id",
                table: "user_board_role",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_board_role_board_id",
                table: "user_board_role",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_board_role_board_role_board_id_board_role_id",
                table: "user_board_role",
                columns: new[] { "board_role_board_id", "board_role_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_user_board_role_roles_board_role_board_id_board_role_id",
                table: "user_board_role",
                columns: new[] { "board_role_board_id", "board_role_id" },
                principalTable: "roles",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
