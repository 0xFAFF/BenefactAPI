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
    [Migration("20190206042728_Users")]
    partial class Users
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
                    b.Property<string>("Email")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("email");

                    b.Property<string>("Hash")
                        .HasColumnName("hash");

                    b.Property<int>("Id")
                        .HasColumnName("id");

                    b.HasKey("Email")
                        .HasName("pk_users");

                    b.ToTable("users");
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
#pragma warning restore 612, 618
        }
    }
}
