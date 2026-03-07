using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesEnquiryHeaderConfiguration : IEntityTypeConfiguration<SalesEnquiryHeader>
    {
        public void Configure(EntityTypeBuilder<SalesEnquiryHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("SalesEnquiryHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PartyId)
                .HasColumnName("PartyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.EnquiryDate)
                .HasColumnName("EnquiryDate")
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(t => t.ContactPerson)
                .HasColumnName("ContactPerson")
                .HasColumnType("varchar(200)")
                .IsRequired(false);

            builder.Property(t => t.ExpectedDeliveryDate)
                .HasColumnName("ExpectedDeliveryDate")
                .HasColumnType("datetimeoffset")
                .IsRequired(false);

            builder.Property(t => t.PaymentTermId)
                .HasColumnName("PaymentTermId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("varchar(500)")
                .IsRequired(false);

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

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            builder.Property(t => t.SalesLeadId)
                .HasColumnName("SalesLeadId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.HasOne(t => t.SalesLead)
                .WithMany(sl => sl.SalesEnquiryHeaders)
                .HasForeignKey(t => t.SalesLeadId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasIndex(t => t.PartyId);
            builder.HasIndex(t => t.SalesLeadId);
        }
    }
}
