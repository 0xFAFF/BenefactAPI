using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class NotNullCardColumnIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cards_columns_column_id",
                table: "cards");

            migrationBuilder.AlterColumn<int>(
                name: "index",
                table: "cards",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "column_id",
                table: "cards",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_cards_columns_column_id",
                table: "cards",
                column: "column_id",
                principalTable: "columns",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cards_columns_column_id",
                table: "cards");

            migrationBuilder.AlterColumn<int>(
                name: "index",
                table: "cards",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "column_id",
                table: "cards",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "fk_cards_columns_column_id",
                table: "cards",
                column: "column_id",
                principalTable: "columns",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
