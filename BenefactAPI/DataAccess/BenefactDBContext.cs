﻿using BenefactAPI.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.NameTranslation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BenefactAPI.DataAccess
{
    public class BenefactDbContext : DbContext
    {
        public DbSet<BoardData> Boards { get; set; }
        public DbSet<UserRole> Roles { get; set; }
        public DbSet<UserData> Users { get; set; }
        public DbSet<CardData> Cards { get; set; }
        public DbSet<CommentData> Comments { get; set; }
        public DbSet<VoteData> Votes { get; set; }
        public DbSet<ColumnData> Columns { get; set; }
        public DbSet<TagData> Tags { get; set; }
        public DbSet<StorageEntry> Files { get; set; }
        public DbSet<AttachmentData> Attachments { get; set; }
        public DbSet<InviteData> Invites { get; set; }
        public DbSet<ActivityData> Activity { get; set; }

        IHostingEnvironment environment;
        public BenefactDbContext(DbContextOptions options, IHostingEnvironment env) : base(options) { environment = env; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (environment.IsDevelopment())
                optionsBuilder.EnableSensitiveDataLogging();

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Board fields
            modelBuilder.ConfigureKey(b => b.Cards);
            modelBuilder.ConfigureKey(b => b.Columns);
            modelBuilder.ConfigureKey(b => b.Tags);
            modelBuilder.ConfigureKey(b => b.Comments);
            modelBuilder.ConfigureKey(b => b.Attachments);
            modelBuilder.ConfigureKey(b => b.Invites);
            modelBuilder.ConfigureKey(b => b.Activity);

            modelBuilder.Entity<BoardData>()
                .HasIndex(bd => bd.UrlName)
                .IsUnique();

            modelBuilder.Entity<BoardData>()
                .HasOne(bd => bd.Creator)
                .WithMany(u => u.CreatedBoards)
                .HasForeignKey(bd => bd.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InviteData>()
                .HasIndex(i => i.Key)
                .IsUnique();

            // Card References
            modelBuilder.CardReference(c => c.Attachments);
            modelBuilder.CardReference(c => c.Comments);
            modelBuilder.CardReference(c => c.Votes);
            modelBuilder.CardReference(c => c.Tags);

            modelBuilder.Entity<CardData>()
                .HasMany(c => c.Activity)
                .WithOne(ac => ac.Card)
                .HasForeignKey(ac => new { ac.BoardId, ac.CardId });

            modelBuilder.Entity<UserData>()
                .HasAlternateKey(ud => ud.Email);
            modelBuilder.Entity<UserData>()
                .HasAlternateKey(ud => ud.Name);

            modelBuilder.Entity<CardData>()
                .HasOne(cd => cd.Author)
                .WithMany(u => u.CreatedCards)
                .HasForeignKey(cd => cd.AuthorId);

            modelBuilder.Entity<CardData>()
                .HasOne(cd => cd.Assignee)
                .WithMany(u => u.AssignedCards)
                .HasForeignKey(cd => cd.AssigneeId);

            modelBuilder.Entity<CardData>()
                .HasOne(cd => cd.Parent)
                .WithMany(cd => cd.Children)
                .HasForeignKey(cd => new { cd.BoardId, cd.ParentId });

            // Privileges
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.BoardId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.Roles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(u => u.Board)
                .WithMany(b => b.Roles)
                .HasForeignKey(u => u.BoardId);

            // Column-Card
            modelBuilder.Entity<CardData>()
                .HasOne(cd => cd.Column)
                .WithMany(co => co.Cards)
                .HasForeignKey(cd => new { cd.BoardId, cd.ColumnId })
                .OnDelete(DeleteBehavior.Restrict);

            // Comments
            modelBuilder.Entity<UserData>()
                .HasMany(u => u.Comments)
                .WithOne(co => co.User);

            // Votes
            modelBuilder.Entity<VoteData>()
                .HasKey(v => new { v.BoardId, v.CardId, v.UserId });

            modelBuilder.Entity<VoteData>()
                .HasOne(v => v.Board)
                .WithMany(bo => bo.Votes)
                .HasForeignKey(v => v.BoardId);

            modelBuilder.Entity<UserData>()
                .HasMany(u => u.Votes)
                .WithOne(vo => vo.User)
                .HasForeignKey(vo => vo.UserId);

            // Card Tags
            modelBuilder.Entity<CardTag>()
                .HasKey(c => new { c.BoardId, c.CardId, c.TagId });

            modelBuilder.Entity<CardTag>()
                .HasOne(ct => ct.Tag)
                .WithMany(t => t.CardTags)
                .HasForeignKey(ct => new { ct.BoardId, ct.TagId });

            // Attachments
            modelBuilder.Entity<UserData>()
                .HasMany(u => u.Attachments)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<AttachmentData>()
                .HasOne(a => a.Storage)
                .WithOne(s => s.Attachment)
                .HasForeignKey<AttachmentData>(a => a.StorageId);

            modelBuilder.Entity<ActivityData>()
                .HasOne(ad => ad.Comment)
                .WithMany(co => co.Activity)
                .HasForeignKey(ad => new { ad.BoardId, ad.CommentId });

            FixSnakeCaseNames(modelBuilder);
        }

        public async Task<bool> DeleteAsync<T>(DbSet<T> set, T delete) where T : class
        {
            Remove(delete);
            try
            {
                await SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync<T>(DbSet<T> set, int id, Func<T, Expression<Func<T, bool>>> orderPredicate = null)
            where T : class, IOrdered, IBoardId
        {
            var existing = await set.FirstOrDefaultAsync(e => e.Id == id && e.BoardId == BoardExtensions.Board.Id);
            if (existing == null) return false;
            if (!await DeleteAsync(set, existing)) return false;
            var filteredSet = set.Where(e => e.BoardId == BoardExtensions.Board.Id);
            if (orderPredicate != null)
                filteredSet = filteredSet.Where(orderPredicate(existing));
            await Order(filteredSet);
            return true;
        }

        public async Task Insert<T>(T value, int? newIndex, IQueryable<T> existingSet) where T : IOrdered
        {
            var max = await existingSet.CountAsync();

            if (value.Index == null)
                value.Index = max;
            if (newIndex == null)
                newIndex = max;
            if (newIndex == value.Index)
                return;
            newIndex = Math.Min(Math.Max(0, newIndex.Value), max);
            var movingEarlier = newIndex < value.Index;
            var startIndex = (movingEarlier ? newIndex : value.Index).Value;
            var endIndex = (movingEarlier ? value.Index : newIndex).Value;
            var greaterList = await existingSet.Where(v => v.Index.Value >= startIndex && v.Index.Value <= endIndex)
                .ToListAsync();
            foreach (var greaterItem in greaterList)
            {
                greaterItem.Index += movingEarlier ? 1 : -1;
            }
            value.Index = newIndex;
            await SaveChangesAsync();
            await Order(existingSet);
        }

        public async Task Order<T>(IQueryable<T> existingSet) where T : IOrdered
        {
            var allItems = await existingSet.OrderBy(v => v.Index).ToListAsync();
            foreach (var tuple in allItems.Select((item, index) => new { item, index }))
                tuple.item.Index = tuple.index;
            await SaveChangesAsync();
        }

        #region Name Conversion
        private static readonly Regex _keysRegex = new Regex("^(PK|FK|IX)_", RegexOptions.Compiled);
        private void FixSnakeCaseNames(ModelBuilder modelBuilder)
        {
            //var mapper = new NpgsqlSnakeCaseNameTranslator();
            //foreach (var table in modelBuilder.Model.GetEntityTypes())
            //{
            //    ConvertToSnake(mapper, table);
            //    foreach (var convert in table.GetProperties().Cast<object>()
            //        .Union(table.GetKeys()).Union(table.GetForeignKeys()).Union(table.GetIndexes()))
            //    {
            //        ConvertToSnake(mapper, convert);
            //    }
            //}
        }
        private void ConvertToSnake(INpgsqlNameTranslator mapper, object entity)
        {
            switch (entity)
            {
                //case IMutableEntityType table:
                //    var relationalTable = table.Relational();
                //    relationalTable.TableName = ConvertGeneralToSnake(mapper, relationalTable.TableName);
                //    if (relationalTable.TableName.StartsWith("asp_net_"))
                //    {
                //        relationalTable.TableName = relationalTable.TableName.Replace("asp_net_", string.Empty);
                //        relationalTable.Schema = "identity";
                //    }

                //    break;
                //case IMutableProperty property:
                //    property.Relational().ColumnName = ConvertGeneralToSnake(mapper, property.Relational().ColumnName);
                //    break;
                //case IMutableKey primaryKey:
                //    primaryKey.Relational().Name = ConvertKeyToSnake(mapper, primaryKey.Relational().Name);
                //    break;
                //case IMutableForeignKey foreignKey:
                //    foreignKey.Relational().Name = ConvertKeyToSnake(mapper, foreignKey.Relational().Name);
                //    break;
                //case IMutableIndex indexKey:
                //    indexKey.Relational().Name = ConvertKeyToSnake(mapper, indexKey.Relational().Name);
                //    break;
                    //default:
                    //    throw new NotImplementedException("Unexpected type was provided to snake case converter");
            }
        }
        private string ConvertKeyToSnake(INpgsqlNameTranslator mapper, string keyName) =>
            ConvertGeneralToSnake(mapper, _keysRegex.Replace(keyName, match => match.Value.ToLower()));
        private string ConvertGeneralToSnake(INpgsqlNameTranslator mapper, string entityName) =>
            mapper.TranslateMemberName(entityName);
        #endregion
    }
}
