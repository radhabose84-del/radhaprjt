using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Data.Configurations
{
    public class HSNMasterConfiguration: IEntityTypeConfiguration<HSNMaster>
    {

        public void Configure(EntityTypeBuilder<HSNMaster> builder)
        {

            var statusConverter = new ValueConverter<Status, bool>(
                   v => v == Status.Active,
                   v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("HSNMaster", "Inventory");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TypeId)
                .HasColumnName("TypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.HSNCode)
                .HasColumnName("HSNCode")
                .HasColumnType("varchar(10)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(250)")
                .IsRequired();

            builder.Property(t => t.GSTCategoryId)
                .HasColumnName("GSTCategoryId")
               .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.GSTPercentage)
                .HasColumnName("GSTPercentage")
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(t => t.CGSTPercentage)
                .HasColumnName("CGSTPercentage")
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(t => t.SGSTPercentage)
                .HasColumnName("SGSTPercentage")
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(t => t.IGSTPercentage)
                .HasColumnName("IGSTPercentage")
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(t => t.ValidFrom)
                .HasColumnName("ValidFrom")
                .HasColumnType("datetimeoffset")
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

            // ✅ Foreign key relationship
            builder.HasOne(h => h.GstCategory)
                   .WithMany(m => m.HSNMasters)
                   .HasForeignKey(h => h.GSTCategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ✅ Indexes (Optional)
            builder.HasIndex(h => h.GSTCategoryId);
            builder.HasIndex(h => h.HSNCode).IsUnique();     

            builder.HasOne(h => h.Type)
                .WithMany(m => m.TypeHSNs)
                .HasForeignKey(h => h.TypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_HSNMaster_MiscMaster_Type");      

        }
        
    }
}