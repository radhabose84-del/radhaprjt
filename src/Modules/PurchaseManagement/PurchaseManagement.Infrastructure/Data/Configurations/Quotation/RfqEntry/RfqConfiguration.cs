using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.Quotation.RfqEntry;

public class RfqConfiguration : IEntityTypeConfiguration<RfqMaster>
{
    public void Configure(EntityTypeBuilder<RfqMaster> b)
    {
         // --- enum -> bit converters (unchanged) ---
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );
        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );
        b.ToTable("RfqMaster", "Purchase");                
        b.HasKey(t => t.Id);
        b.Property(t => t.Id)
            .ValueGeneratedOnAdd();      

        b.Property(m => m.UnitId)  // Foreign Key column
                .HasColumnName("UnitId")
                .HasColumnType("int")  // Set as int
                .IsRequired();


        b.Property(x => x.RfqCode).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.RfqCode).IsUnique();
        
 // 🔹 Status relation
        b.Property(x => x.RfqStatusId).IsRequired();
        b.HasOne(x => x.RfqStatus)
            .WithMany(m => m.RfqStatuses)
            .HasForeignKey(x => x.RfqStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔹 Initiation relation
        b.Property(x => x.InitiationTypeId).IsRequired(false);
        b.HasOne(x => x.InitiationType)
            .WithMany(m => m.RfqInitiationTypes)
            .HasForeignKey(x => x.InitiationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.IndentId);        
        b.Property(x => x.LastSubmitDate).HasColumnName("LastSubmitDate").HasColumnType("date").IsRequired(false);      

        b.HasMany(x => x.Items)
            .WithOne(i => i.Rfq)
            .HasForeignKey(i => i.RfqId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Suppliers)
            .WithOne(s => s.Rfq)
            .HasForeignKey(s => s.RfqId)
            .OnDelete(DeleteBehavior.Cascade);

          // --- BaseEntity columns (unchanged) ---
        b.Property(x => x.IsActive).HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
        b.Property(x => x.IsDeleted).HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
        b.Property(x => x.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int").IsRequired();
        b.Property(x => x.CreatedDate).HasColumnName("CreatedDate").HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int").IsRequired(false);
        b.Property(x => x.ModifiedDate).HasColumnName("ModifiedDate").HasColumnType("datetimeoffset").IsRequired(false);
        b.Property(x => x.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(50)").IsRequired(false);
        b.Property(x => x.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)").IsRequired(false);
    }
}
