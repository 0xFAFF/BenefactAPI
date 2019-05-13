using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BenefactAPI.Migrations
{
    public partial class Invites : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "default_privilege",
                table: "boards",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "boards",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "invites",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    board_id = table.Column<int>(nullable: false),
                    key = table.Column<string>(maxLength: 10, nullable: false),
                    privilege = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invites", x => new { x.board_id, x.id });
                    table.ForeignKey(
                        name: "fk_invites_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_invites_key",
                table: "invites",
                column: "key",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invites");

            migrationBuilder.DropColumn(
                name: "default_privilege",
                table: "boards");

            migrationBuilder.DropColumn(
                name: "description",
                table: "boards");
        }
    }
}
