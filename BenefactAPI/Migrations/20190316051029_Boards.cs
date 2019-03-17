using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BenefactAPI.Migrations
{
    public partial class Boards : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "boards",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_boards", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "boards",
                columns: new string[] { },
                values: new object[] { });

            migrationBuilder.AddColumn<int>(
                name: "board_id",
                table: "tags",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "board_id",
                table: "columns",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "board_id",
                table: "cards",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "board_id",
                table: "tags",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "board_id",
                table: "columns",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "board_id",
                table: "cards",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_tags_board_id",
                table: "tags",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_columns_board_id",
                table: "columns",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_cards_board_id",
                table: "cards",
                column: "board_id");

            migrationBuilder.AddForeignKey(
                name: "fk_cards_boards_board_id",
                table: "cards",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_columns_boards_board_id",
                table: "columns",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tags_boards_board_id",
                table: "tags",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cards_boards_board_id",
                table: "cards");

            migrationBuilder.DropForeignKey(
                name: "fk_columns_boards_board_id",
                table: "columns");

            migrationBuilder.DropForeignKey(
                name: "fk_tags_boards_board_id",
                table: "tags");

            migrationBuilder.DropTable(
                name: "boards");

            migrationBuilder.DropIndex(
                name: "ix_tags_board_id",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "ix_columns_board_id",
                table: "columns");

            migrationBuilder.DropIndex(
                name: "ix_cards_board_id",
                table: "cards");

            migrationBuilder.DropColumn(
                name: "board_id",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "board_id",
                table: "columns");

            migrationBuilder.DropColumn(
                name: "board_id",
                table: "cards");
        }
    }
}
