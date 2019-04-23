using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BenefactAPI.Migrations
{
    public partial class UserRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("cards");
            migrationBuilder.DropTable(
                name: "user_privilege");

            migrationBuilder.DropColumn(
                name: "default_privileges",
                table: "boards");

            migrationBuilder.AddColumn<bool>(
                name: "allow_contribution",
                table: "columns",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "author_id",
                table: "cards",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "board_role",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    board_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(nullable: true),
                    privilege = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board_role", x => new { x.board_id, x.id });
                    table.ForeignKey(
                        name: "fk_board_role_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_board_role_boards_id",
                        column: x => x.id,
                        principalTable: "boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_board_role",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    board_id = table.Column<int>(nullable: false),
                    board_role_board_id = table.Column<int>(nullable: true),
                    board_role_id = table.Column<int>(nullable: true)
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
                        name: "fk_user_board_role_board_role_board_role_board_id_board_role_id",
                        columns: x => new { x.board_role_board_id, x.board_role_id },
                        principalTable: "board_role",
                        principalColumns: new[] { "board_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cards_author_id",
                table: "cards",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_board_role_id",
                table: "board_role",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_board_role_board_id",
                table: "user_board_role",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_board_role_board_role_board_id_board_role_id",
                table: "user_board_role",
                columns: new[] { "board_role_board_id", "board_role_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_cards_users_author_id",
                table: "cards",
                column: "author_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cards_users_author_id",
                table: "cards");

            migrationBuilder.DropTable(
                name: "user_board_role");

            migrationBuilder.DropTable(
                name: "board_role");

            migrationBuilder.DropIndex(
                name: "ix_cards_author_id",
                table: "cards");

            migrationBuilder.DropColumn(
                name: "allow_contribution",
                table: "columns");

            migrationBuilder.DropColumn(
                name: "author_id",
                table: "cards");

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
    }
}
