using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BenefactAPI.Migrations
{
    public partial class Activity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activity",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    board_id = table.Column<int>(nullable: false),
                    user_id = table.Column<int>(nullable: false),
                    type = table.Column<int>(nullable: false),
                    message = table.Column<string>(nullable: true),
                    time = table.Column<double>(nullable: false),
                    card_id = table.Column<int>(nullable: true),
                    comment_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity", x => new { x.board_id, x.id });
                    table.ForeignKey(
                        name: "fk_activity_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_activity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_activity_cards_board_id_card_id",
                        columns: x => new { x.board_id, x.card_id },
                        principalTable: "cards",
                        principalColumns: new[] { "board_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_activity_comments_board_id_comment_id",
                        columns: x => new { x.board_id, x.comment_id },
                        principalTable: "comments",
                        principalColumns: new[] { "board_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_user_id",
                table: "activity",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_board_id_card_id",
                table: "activity",
                columns: new[] { "board_id", "card_id" });

            migrationBuilder.CreateIndex(
                name: "ix_activity_board_id_comment_id",
                table: "activity",
                columns: new[] { "board_id", "comment_id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity");
        }
    }
}
