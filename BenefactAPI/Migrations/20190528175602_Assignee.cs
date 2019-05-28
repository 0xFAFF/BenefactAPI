using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class Assignee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "assignee_id",
                table: "cards",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_cards_assignee_id",
                table: "cards",
                column: "assignee_id");

            migrationBuilder.AddForeignKey(
                name: "fk_cards_users_assignee_id",
                table: "cards",
                column: "assignee_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cards_users_assignee_id",
                table: "cards");

            migrationBuilder.DropIndex(
                name: "ix_cards_assignee_id",
                table: "cards");

            migrationBuilder.DropColumn(
                name: "assignee_id",
                table: "cards");
        }
    }
}
