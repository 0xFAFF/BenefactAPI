﻿// <auto-generated />
using System;
using BenefactAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BenefactAPI.Migrations
{
    [DbContext(typeof(BenefactDbContext))]
    [Migration("20190213043357_Votes")]
    partial class Votes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.1-servicing-10028")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("BenefactAPI.Controllers.CardData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int?>("ColumnId")
                        .IsRequired()
                        .HasColumnName("column_id");

                    b.Property<string>("Description")
                        .HasColumnName("description");

                    b.Property<int?>("Index")
                        .IsRequired()
                        .HasColumnName("index");

                    b.Property<string>("Title")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("pk_cards");

                    b.HasIndex("ColumnId")
                        .HasName("ix_cards_column_id");

                    b.ToTable("cards");
                });

            modelBuilder.Entity("BenefactAPI.Controllers.CardTag", b =>
                {
                    b.Property<int>("CardId")
                        .HasColumnName("card_id");

                    b.Property<int>("TagId")
                        .HasColumnName("tag_id");

                    b.HasKey("CardId", "TagId")
                        .HasName("pk_card_tag");

                    b.HasIndex("TagId")
                        .HasName("ix_card_tag_tag_id");

                    b.ToTable("card_tag");
                });

            modelBuilder.Entity("BenefactAPI.Controllers.ColumnData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int?>("Index")
                        .HasColumnName("index");

                    b.Property<string>("Title")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("pk_columns");

                    b.ToTable("columns");
                });

            modelBuilder.Entity("BenefactAPI.Controllers.CommentData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int>("CardId")
                        .HasColumnName("card_id");

                    b.Property<string>("Text")
                        .HasColumnName("text");

                    b.Property<int>("UserId")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_comments");

                    b.HasIndex("CardId")
                        .HasName("ix_comments_card_id");

                    b.HasIndex("UserId")
                        .HasName("ix_comments_user_id");

                    b.ToTable("comments");
                });

            modelBuilder.Entity("BenefactAPI.Controllers.TagData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Character")
                        .HasColumnName("character");

                    b.Property<string>("Color")
                        .HasColumnName("color");

                    b.Property<string>("Name")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_tags");

                    b.ToTable("tags");
                });

            modelBuilder.Entity("BenefactAPI.Controllers.UserData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnName("email");

                    b.Property<string>("Hash")
                        .HasColumnName("hash");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasAlternateKey("Email")
                        .HasName("ak_users_email");

                    b.ToTable("users");
                });

            modelBuilder.Entity("BenefactAPI.Controllers.VoteData", b =>
                {
                    b.Property<int>("CardId")
                        .HasColumnName("card_id");

                    b.Property<int>("UserId")
                        .HasColumnName("user_id");

                    b.Property<int>("Count")
                        .HasColumnName("count");

                    b.HasKey("CardId", "UserId")
                        .HasName("pk_votes");

                    b.HasIndex("UserId")
                        .HasName("ix_votes_user_id");

                    b.ToTable("votes");
                });

            modelBuilder.Entity("BenefactAPI.Controllers.CardData", b =>
                {
                    b.HasOne("BenefactAPI.Controllers.ColumnData", "Column")
                        .WithMany("Cards")
                        .HasForeignKey("ColumnId")
                        .HasConstraintName("fk_cards_columns_column_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BenefactAPI.Controllers.CardTag", b =>
                {
                    b.HasOne("BenefactAPI.Controllers.CardData", "Card")
                        .WithMany("Tags")
                        .HasForeignKey("CardId")
                        .HasConstraintName("fk_card_tag_cards_card_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("BenefactAPI.Controllers.TagData", "Tag")
                        .WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("fk_card_tag_tags_tag_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BenefactAPI.Controllers.CommentData", b =>
                {
                    b.HasOne("BenefactAPI.Controllers.CardData", "Card")
                        .WithMany("Comments")
                        .HasForeignKey("CardId")
                        .HasConstraintName("fk_comments_cards_card_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("BenefactAPI.Controllers.UserData", "User")
                        .WithMany("Comments")
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_comments_users_user_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BenefactAPI.Controllers.VoteData", b =>
                {
                    b.HasOne("BenefactAPI.Controllers.CardData", "Card")
                        .WithMany("Votes")
                        .HasForeignKey("CardId")
                        .HasConstraintName("fk_votes_cards_card_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("BenefactAPI.Controllers.UserData", "User")
                        .WithMany("Votes")
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_votes_users_user_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
