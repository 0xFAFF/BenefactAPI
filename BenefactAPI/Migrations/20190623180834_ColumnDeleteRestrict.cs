using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class ColumnDeleteRestrict : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cards_columns_board_id_column_id",
                table: "cards");

            migrationBuilder.AddForeignKey(
                name: "fk_cards_columns_board_id_column_id",
                table: "cards",
                columns: new[] { "board_id", "column_id" },
                principalTable: "columns",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cards_columns_board_id_column_id",
                table: "cards");

            migrationBuilder.AddForeignKey(
                name: "fk_cards_columns_board_id_column_id",
                table: "cards",
                columns: new[] { "board_id", "column_id" },
                principalTable: "columns",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
