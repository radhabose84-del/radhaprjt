using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations
{
    public class IndentDetailConfiguration : IEntityTypeConfiguration<IndentDetail>
    {
        public void Configure(EntityTypeBuilder<IndentDetail> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
             v => v == Status.Active,
             v => v ? Status.Active : Status.Inactive
         );


            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("IndentDetail", "Purchase");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.IndentHeaderId)
                    .HasColumnName("IndentHeaderId")
                    .HasColumnType("int")
                    .IsRequired();

            builder.Property(m => m.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(m => m.ItemCategoryId)
                .HasColumnName("ItemCategoryId")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(m => m.ItemUOMId)
                .HasColumnName("ItemUOMId")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(m => m.Rate)
                .HasColumnName("Rate")
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(m => m.QuantityRequired)
                   .HasColumnName("QuantityRequired")
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            // builder.Property(m => m.ApprovedQuantity)
            // .HasColumnName("ApprovedQuantity")
            // .HasColumnType("decimal(18,2)")
            // .IsRequired(false);
            builder.Property(m => m.IsRFQDone)
                   .HasColumnName("IsRFQDone")
                   .IsRequired();


            builder.Property(m => m.RequiredDate)
                   .HasColumnName("RequiredDate")
                   .HasColumnType("date")
                   .IsRequired();

            builder.Property(m => m.TotalEstimatedCost)
                   .HasColumnName("TotalEstimatedCost")
                   .HasColumnType("decimal(18,2)")
                   .IsRequired(false);

            builder.Property(m => m.PRConsumptionDays)
        .HasColumnName("PRConsumptionDays")
        .HasColumnType("int")
        .IsRequired();

            builder.Property(m => m.Remark)
        .HasColumnName("Remark")
        .HasColumnType("varchar(max)")
        .IsRequired();

        builder.Property(m => m.POQty)
                   .HasColumnName("POQty")
                   .HasColumnType("decimal(18,2)")
                   .IsRequired(false);

            builder.Property(m => m.StatusId)
                       .HasColumnName("StatusId")
                       .HasColumnType("int")
                       .IsRequired();


            builder.Property(b => b.IsActive)
                    .HasColumnName("IsActive")
                    .HasColumnType("bit")
                    .HasConversion(statusConverter)
                    .IsRequired();

            builder.Property(b => b.IsDeleted)
                  .HasColumnName("IsDeleted")
                  .HasColumnType("bit")
                  .HasConversion(isDeleteConverter)
                  .IsRequired();

            builder.Property(b => b.CreatedByName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");


            builder.Property(b => b.CreatedIP)
                    .IsRequired()
                    .HasColumnType("varchar(20)");

            builder.Property(b => b.ModifiedByName)
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                    .HasColumnType("varchar(20)");

            builder.HasOne(ac => ac.IndentHeader)
        .WithMany(am => am.IndentDetails)
        .HasForeignKey(ac => ac.IndentHeaderId)
        .OnDelete(DeleteBehavior.NoAction);
                
                 builder.HasOne(ac => ac.Status)
                .WithMany(am => am.StatusDetail)
                .HasForeignKey(ac => ac.StatusId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}