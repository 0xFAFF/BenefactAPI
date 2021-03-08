using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class CardLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "parent_id",
                table: "cards",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_cards_board_id_parent_id",
                table: "cards",
                columns: new[] { "board_id", "parent_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_cards_cards_board_id_parent_id",
                table: "cards",
                columns: new[] { "board_id", "parent_id" },
                principalTable: "cards",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cards_cards_board_id_parent_id",
                table: "cards");

            migrationBuilder.DropIndex(
                name: "ix_cards_board_id_parent_id",
                table: "cards");

            migrationBuilder.DropColumn(
                name: "parent_id",
                table: "cards");
        }
    }
}
