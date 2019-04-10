using BenefactAPI.Controllers;
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
        public DbSet<UserData> Users { get; set; }
        public DbSet<CardData> Cards { get; set; }
        public DbSet<CommentData> Comments { get; set; }
        public DbSet<VoteData> Votes { get; set; }
        public DbSet<ColumnData> Columns { get; set; }
        public DbSet<TagData> Tags { get; set; }
        public DbSet<StorageEntry> Files { get; set; }
        public DbSet<AttachmentData> Attachments { get; set; }

        public BenefactDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Boards
            modelBuilder.Entity<CardData>()
                .HasOne(cd => cd.Board)
                .WithMany(bo => bo.Cards)
                .HasForeignKey(cd => cd.BoardId);

            modelBuilder.Entity<TagData>()
                .HasOne(t => t.Board)
                .WithMany(bo => bo.Tags)
                .HasForeignKey(t => t.BoardId);

            modelBuilder.Entity<ColumnData>()
                .HasOne(co => co.Board)
                .WithMany(bo => bo.Columns)
                .HasForeignKey(co => co.BoardId);

            // Privileges
            modelBuilder.Entity<UserPrivilege>()
                .HasKey(u => new { u.UserId, u.BoardId });

            modelBuilder.Entity<BoardData>()
                .Property(b => b.DefaultPrivileges)
                .HasDefaultValue(Privileges.View);

            modelBuilder.Entity<BoardData>()
                .HasMany(b => b.Users)
                .WithOne(u => u.Board)
                .HasForeignKey(u => u.BoardId);

            modelBuilder.Entity<UserData>()
                .HasMany(u => u.Privileges)
                .WithOne(up => up.User)
                .HasForeignKey(up => up.UserId);

            // Column-Card
            modelBuilder.Entity<CardData>()
                .HasOne(cd => cd.Column)
                .WithMany(co => co.Cards)
                .HasForeignKey(cd => cd.ColumnId);

            // Comments
            modelBuilder.Entity<CardData>()
                .HasMany(cd => cd.Comments)
                .WithOne(co => co.Card)
                .HasForeignKey(co => co.CardId);

            modelBuilder.Entity<UserData>()
                .HasMany(u => u.Comments)
                .WithOne(co => co.User);

            // Votes
            modelBuilder.Entity<VoteData>()
                .HasKey(v => new { v.CardId, v.UserId });

            modelBuilder.Entity<CardData>()
                .HasMany(cd => cd.Votes)
                .WithOne(vo => vo.Card)
                .HasForeignKey(vo => vo.CardId);

            modelBuilder.Entity<UserData>()
                .HasMany(u => u.Votes)
                .WithOne(vo => vo.User)
                .HasForeignKey(vo => vo.UserId);

            // Card Tags
            modelBuilder.Entity<CardTag>()
                .HasKey(c => new { c.CardId, c.TagId });

            modelBuilder.Entity<CardTag>()
                .HasOne(cc => cc.Card)
                .WithMany(ca => ca.Tags)
                .HasForeignKey(cc => cc.CardId);

            modelBuilder.Entity<UserData>()
                .HasAlternateKey(ud => ud.Email);

            // Attachments
            modelBuilder.Entity<CardData>()
                .HasMany(c => c.Attachments)
                .WithOne(a => a.Card)
                .HasForeignKey(a => a.CardId);

            modelBuilder.Entity<UserData>()
                .HasMany(u => u.Attachments)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<AttachmentData>()
                .HasOne(a => a.Storage)
                .WithOne(s => s.Attachment)
                .HasForeignKey<AttachmentData>(a => a.StorageId);

            FixSnakeCaseNames(modelBuilder);
        }

        public async Task<bool> Delete<T>(DbSet<T> set, T delete) where T : class
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

        public async Task<bool> DeleteAndOrder<T>(DbSet<T> set, int id, Func<T, Expression<Func<T, bool>>> orderPredicate)
            where T : class, IOrdered, IId
        {
            var existing = await set.FirstOrDefaultAsync(e => e.Id == id);
            if (existing == null) return false;
            if (!await Delete(set, existing)) return false;
            await Order(set.Where(orderPredicate(existing)));
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
            var mapper = new NpgsqlSnakeCaseNameTranslator();
            foreach (var table in modelBuilder.Model.GetEntityTypes())
            {
                ConvertToSnake(mapper, table);
                foreach (var convert in table.GetProperties().Cast<object>()
                    .Union(table.GetKeys()).Union(table.GetForeignKeys()).Union(table.GetIndexes()))
                {
                    ConvertToSnake(mapper, convert);
                }
            }
        }
        private void ConvertToSnake(INpgsqlNameTranslator mapper, object entity)
        {
            switch (entity)
            {
                case IMutableEntityType table:
                    var relationalTable = table.Relational();
                    relationalTable.TableName = ConvertGeneralToSnake(mapper, relationalTable.TableName);
                    if (relationalTable.TableName.StartsWith("asp_net_"))
                    {
                        relationalTable.TableName = relationalTable.TableName.Replace("asp_net_", string.Empty);
                        relationalTable.Schema = "identity";
                    }

                    break;
                case IMutableProperty property:
                    property.Relational().ColumnName = ConvertGeneralToSnake(mapper, property.Relational().ColumnName);
                    break;
                case IMutableKey primaryKey:
                    primaryKey.Relational().Name = ConvertKeyToSnake(mapper, primaryKey.Relational().Name);
                    break;
                case IMutableForeignKey foreignKey:
                    foreignKey.Relational().Name = ConvertKeyToSnake(mapper, foreignKey.Relational().Name);
                    break;
                case IMutableIndex indexKey:
                    indexKey.Relational().Name = ConvertKeyToSnake(mapper, indexKey.Relational().Name);
                    break;
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
