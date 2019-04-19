using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class Filename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "files",
                keyColumns: new string[] { },
                keyValues: new object[] { });

            migrationBuilder.DeleteData(
                table: "attachments",
                keyColumns: new string[] { },
                keyValues: new object[] { });

            migrationBuilder.DropColumn(
                name: "content_type",
                table: "files");

            migrationBuilder.AlterColumn<byte[]>(
                name: "data",
                table: "files",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "attachments",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "attachments",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_type",
                table: "attachments");

            migrationBuilder.DropColumn(
                name: "name",
                table: "attachments");

            migrationBuilder.AlterColumn<byte[]>(
                name: "data",
                table: "files",
                nullable: true,
                oldClrType: typeof(byte[]));

            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "files",
                nullable: true);
        }
    }
}
