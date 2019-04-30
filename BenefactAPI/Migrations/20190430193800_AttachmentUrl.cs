using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class AttachmentUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("attachments");
            migrationBuilder.DeleteData("files");
            migrationBuilder.DropForeignKey(
                name: "fk_attachments_files_storage_id",
                table: "attachments");

            migrationBuilder.AddColumn<int>(
                name: "creator_id",
                table: "boards",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "storage_id",
                table: "attachments",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "content_type",
                table: "attachments",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "preview",
                table: "attachments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "url",
                table: "attachments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_boards_creator_id",
                table: "boards",
                column: "creator_id");

            migrationBuilder.AddForeignKey(
                name: "fk_attachments_files_storage_id",
                table: "attachments",
                column: "storage_id",
                principalTable: "files",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_boards_users_creator_id",
                table: "boards",
                column: "creator_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("attachments");
            migrationBuilder.DeleteData("files");
            migrationBuilder.DropForeignKey(
                name: "fk_attachments_files_storage_id",
                table: "attachments");

            migrationBuilder.DropForeignKey(
                name: "fk_boards_users_creator_id",
                table: "boards");

            migrationBuilder.DropIndex(
                name: "ix_boards_creator_id",
                table: "boards");

            migrationBuilder.DropColumn(
                name: "creator_id",
                table: "boards");

            migrationBuilder.DropColumn(
                name: "preview",
                table: "attachments");

            migrationBuilder.DropColumn(
                name: "url",
                table: "attachments");

            migrationBuilder.AlterColumn<int>(
                name: "storage_id",
                table: "attachments",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "content_type",
                table: "attachments",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_attachments_files_storage_id",
                table: "attachments",
                column: "storage_id",
                principalTable: "files",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
