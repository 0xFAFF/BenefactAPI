using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class UserPrivileges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "default_privileges",
                table: "boards",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "user_privilege",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    board_id = table.Column<int>(nullable: false),
                    privilege = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_privilege", x => new { x.user_id, x.board_id });
                    table.ForeignKey(
                        name: "fk_user_privilege_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_privilege_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_privilege_board_id",
                table: "user_privilege",
                column: "board_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_privilege");

            migrationBuilder.DropColumn(
                name: "default_privileges",
                table: "boards");
        }
    }
}
