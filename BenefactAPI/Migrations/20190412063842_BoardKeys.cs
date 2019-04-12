using Microsoft.EntityFrameworkCore.Migrations;

namespace BenefactAPI.Migrations
{
    public partial class BoardKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_attachments_cards_card_id",
                table: "attachments");

            migrationBuilder.DropForeignKey(
                name: "fk_card_tag_cards_card_id",
                table: "card_tag");

            migrationBuilder.DropForeignKey(
                name: "fk_card_tag_tags_tag_id",
                table: "card_tag");

            migrationBuilder.DropForeignKey(
                name: "fk_cards_columns_column_id",
                table: "cards");

            migrationBuilder.DropForeignKey(
                name: "fk_comments_cards_card_id",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "fk_votes_cards_card_id",
                table: "votes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_votes",
                table: "votes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tags",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "ix_tags_board_id",
                table: "tags");

            migrationBuilder.DropPrimaryKey(
                name: "pk_comments",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "ix_comments_card_id",
                table: "comments");

            migrationBuilder.DropPrimaryKey(
                name: "pk_columns",
                table: "columns");

            migrationBuilder.DropIndex(
                name: "ix_columns_board_id",
                table: "columns");

            migrationBuilder.DropPrimaryKey(
                name: "pk_cards",
                table: "cards");

            migrationBuilder.DropIndex(
                name: "ix_cards_board_id",
                table: "cards");

            migrationBuilder.DropIndex(
                name: "ix_cards_column_id",
                table: "cards");

            migrationBuilder.DropPrimaryKey(
                name: "pk_card_tag",
                table: "card_tag");

            migrationBuilder.DropIndex(
                name: "ix_card_tag_tag_id",
                table: "card_tag");

            migrationBuilder.DropPrimaryKey(
                name: "pk_attachments",
                table: "attachments");

            migrationBuilder.DropIndex(
                name: "ix_attachments_card_id",
                table: "attachments");

            migrationBuilder.AddColumn<int>(
                name: "board_id",
                table: "votes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "board_id",
                table: "comments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "board_id",
                table: "card_tag",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "board_id",
                table: "attachments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "pk_votes",
                table: "votes",
                columns: new[] { "board_id", "card_id", "user_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_tags",
                table: "tags",
                columns: new[] { "board_id", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_comments",
                table: "comments",
                columns: new[] { "board_id", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_columns",
                table: "columns",
                columns: new[] { "board_id", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_cards",
                table: "cards",
                columns: new[] { "board_id", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_card_tag",
                table: "card_tag",
                columns: new[] { "board_id", "card_id", "tag_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_attachments",
                table: "attachments",
                columns: new[] { "board_id", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_comments_board_id_card_id",
                table: "comments",
                columns: new[] { "board_id", "card_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cards_board_id_column_id",
                table: "cards",
                columns: new[] { "board_id", "column_id" });

            migrationBuilder.CreateIndex(
                name: "ix_card_tag_board_id_tag_id",
                table: "card_tag",
                columns: new[] { "board_id", "tag_id" });

            migrationBuilder.CreateIndex(
                name: "ix_attachments_board_id_card_id",
                table: "attachments",
                columns: new[] { "board_id", "card_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_attachments_boards_board_id",
                table: "attachments",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_attachments_cards_board_id_card_id",
                table: "attachments",
                columns: new[] { "board_id", "card_id" },
                principalTable: "cards",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_card_tag_boards_board_id",
                table: "card_tag",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_card_tag_cards_board_id_card_id",
                table: "card_tag",
                columns: new[] { "board_id", "card_id" },
                principalTable: "cards",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_card_tag_tags_board_id_tag_id",
                table: "card_tag",
                columns: new[] { "board_id", "tag_id" },
                principalTable: "tags",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_cards_columns_board_id_column_id",
                table: "cards",
                columns: new[] { "board_id", "column_id" },
                principalTable: "columns",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_comments_boards_board_id",
                table: "comments",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_comments_cards_board_id_card_id",
                table: "comments",
                columns: new[] { "board_id", "card_id" },
                principalTable: "cards",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_votes_boards_board_id",
                table: "votes",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_votes_cards_board_id_card_id",
                table: "votes",
                columns: new[] { "board_id", "card_id" },
                principalTable: "cards",
                principalColumns: new[] { "board_id", "id" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_attachments_boards_board_id",
                table: "attachments");

            migrationBuilder.DropForeignKey(
                name: "fk_attachments_cards_board_id_card_id",
                table: "attachments");

            migrationBuilder.DropForeignKey(
                name: "fk_card_tag_boards_board_id",
                table: "card_tag");

            migrationBuilder.DropForeignKey(
                name: "fk_card_tag_cards_board_id_card_id",
                table: "card_tag");

            migrationBuilder.DropForeignKey(
                name: "fk_card_tag_tags_board_id_tag_id",
                table: "card_tag");

            migrationBuilder.DropForeignKey(
                name: "fk_cards_columns_board_id_column_id",
                table: "cards");

            migrationBuilder.DropForeignKey(
                name: "fk_comments_boards_board_id",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "fk_comments_cards_board_id_card_id",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "fk_votes_boards_board_id",
                table: "votes");

            migrationBuilder.DropForeignKey(
                name: "fk_votes_cards_board_id_card_id",
                table: "votes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_votes",
                table: "votes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tags",
                table: "tags");

            migrationBuilder.DropPrimaryKey(
                name: "pk_comments",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "ix_comments_board_id_card_id",
                table: "comments");

            migrationBuilder.DropPrimaryKey(
                name: "pk_columns",
                table: "columns");

            migrationBuilder.DropPrimaryKey(
                name: "pk_cards",
                table: "cards");

            migrationBuilder.DropIndex(
                name: "ix_cards_board_id_column_id",
                table: "cards");

            migrationBuilder.DropPrimaryKey(
                name: "pk_card_tag",
                table: "card_tag");

            migrationBuilder.DropIndex(
                name: "ix_card_tag_board_id_tag_id",
                table: "card_tag");

            migrationBuilder.DropPrimaryKey(
                name: "pk_attachments",
                table: "attachments");

            migrationBuilder.DropIndex(
                name: "ix_attachments_board_id_card_id",
                table: "attachments");

            migrationBuilder.DropColumn(
                name: "board_id",
                table: "votes");

            migrationBuilder.DropColumn(
                name: "board_id",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "board_id",
                table: "card_tag");

            migrationBuilder.DropColumn(
                name: "board_id",
                table: "attachments");

            migrationBuilder.AddPrimaryKey(
                name: "pk_votes",
                table: "votes",
                columns: new[] { "card_id", "user_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_tags",
                table: "tags",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_comments",
                table: "comments",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_columns",
                table: "columns",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_cards",
                table: "cards",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_card_tag",
                table: "card_tag",
                columns: new[] { "card_id", "tag_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_attachments",
                table: "attachments",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_board_id",
                table: "tags",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_card_id",
                table: "comments",
                column: "card_id");

            migrationBuilder.CreateIndex(
                name: "ix_columns_board_id",
                table: "columns",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_cards_board_id",
                table: "cards",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_cards_column_id",
                table: "cards",
                column: "column_id");

            migrationBuilder.CreateIndex(
                name: "ix_card_tag_tag_id",
                table: "card_tag",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_attachments_card_id",
                table: "attachments",
                column: "card_id");

            migrationBuilder.AddForeignKey(
                name: "fk_attachments_cards_card_id",
                table: "attachments",
                column: "card_id",
                principalTable: "cards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_card_tag_cards_card_id",
                table: "card_tag",
                column: "card_id",
                principalTable: "cards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_card_tag_tags_tag_id",
                table: "card_tag",
                column: "tag_id",
                principalTable: "tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_cards_columns_column_id",
                table: "cards",
                column: "column_id",
                principalTable: "columns",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_comments_cards_card_id",
                table: "comments",
                column: "card_id",
                principalTable: "cards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_votes_cards_card_id",
                table: "votes",
                column: "card_id",
                principalTable: "cards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
