using BenefactAPI.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Npgsql.NameTranslation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BenefactAPI.DataAccess
{
    public class BenefactDBContext : DbContext
    {
        public DbSet<CardData> Cards { get; set; }
        public DbSet<ColumnData> Columns { get; set; }
        public DbSet<TagData> Tags { get; set; }

        public BenefactDBContext(DbContextOptions options) : base(options) { }

        private static readonly Regex _keysRegex = new Regex("^(PK|FK|IX)_", RegexOptions.Compiled);
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CardData>()
                .HasOne(cd => cd.Column)
                .WithMany(co => co.Cards)
                .HasForeignKey(cd => cd.ColumnId);

            modelBuilder.Entity<CardTag>()
                .HasKey(c => new { c.CardId, c.TagId });

            modelBuilder.Entity<CardTag>()
                .HasOne(cc => cc.Card)
                .WithMany(ca => ca.Tags)
                .HasForeignKey(cc => cc.CardId);

            FixSnakeCaseNames(modelBuilder);
        }

        #region Name Conversion

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
                default:
                    throw new NotImplementedException("Unexpected type was provided to snake case converter");
            }
        }

        private string ConvertKeyToSnake(INpgsqlNameTranslator mapper, string keyName) =>
            ConvertGeneralToSnake(mapper, _keysRegex.Replace(keyName, match => match.Value.ToLower()));

        private string ConvertGeneralToSnake(INpgsqlNameTranslator mapper, string entityName) =>
            mapper.TranslateMemberName(entityName);
        #endregion
    }
}
