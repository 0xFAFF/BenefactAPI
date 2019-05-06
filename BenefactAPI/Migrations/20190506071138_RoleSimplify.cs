using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BenefactAPI.Migrations
{
    public partial class RoleSimplify : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("roles");

            migrationBuilder.DropTable(
                name: "user_board_role");

            migrationBuilder.DropPrimaryKey(
                name: "pk_roles",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "id",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "name",
                table: "roles");

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "roles",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "pk_roles",
                table: "roles",
                columns: new[] { "user_id", "board_id" });

            migrationBuilder.CreateIndex(
                name: "ix_roles_board_id",
                table: "roles",
                column: "board_id");

            migrationBuilder.AddForeignKey(
                name: "fk_roles_users_user_id",
                table: "roles",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_roles_users_user_id",
                table: "roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_roles",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "ix_roles_board_id",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "roles");

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "roles",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "roles",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_roles",
                table: "roles",
                columns: new[] { "board_id", "id" });

            migrationBuilder.CreateTable(
                name: "user_board_role",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    board_id = table.Column<int>(nullable: false),
                    board_role_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_board_role", x => new { x.user_id, x.board_id });
                    table.ForeignKey(
                        name: "fk_user_board_role_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_board_role_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_board_role",
                        columns: x => new { x.board_id, x.board_role_id },
                        principalTable: "roles",
                        principalColumns: new[] { "board_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ik_board_role",
                table: "user_board_role",
                columns: new[] { "board_id", "board_role_id" });
        }
    }
}
