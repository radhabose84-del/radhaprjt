using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class GlAccountFavouriteConfiguration : IEntityTypeConfiguration<GlAccountFavourite>
    {
        public void Configure(EntityTypeBuilder<GlAccountFavourite> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("GlAccountFavourite", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.GlAccountMasterId).HasColumnName("GlAccountMasterId").HasColumnType("int").IsRequired();
            builder.Property(t => t.UserId).HasColumnName("UserId").HasColumnType("int").IsRequired();
            builder.Property(t => t.CompanyId).HasColumnName("CompanyId").HasColumnType("int").IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Fast "my favourites" lookups.
            builder.HasIndex(t => new { t.UserId, t.CompanyId });

            // One ACTIVE favourite per user+company+account (filtered so a re-star after un-star is allowed).
            builder.HasIndex(t => new { t.UserId, t.CompanyId, t.GlAccountMasterId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Same-module FK → Finance.GlAccountMaster (Restrict: keep the favourite's account intact).
            builder.HasOne(t => t.GlAccountMaster)
                .WithMany()
                .HasForeignKey(t => t.GlAccountMasterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
