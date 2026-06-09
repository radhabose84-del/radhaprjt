using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class SalesTypeConfiguration : IEntityTypeConfiguration<SalesType>
    {
        public void Configure(EntityTypeBuilder<SalesType> builder)
        {
            builder.ToTable("SalesType", "Party");

            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            // PartyId - FK to PartyMaster (required)
            builder.Property(m => m.PartyId)
                .HasColumnName("PartyId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(m => m.Party)
                .WithMany(t => t.SalesTypes)
                .HasForeignKey(m => m.PartyId)
                .OnDelete(DeleteBehavior.Restrict);

            // SalesSegmentId - plain int, no FK reference
            builder.Property(m => m.SalesSegmentId)
                .HasColumnName("SalesSegmentId")
                .HasColumnType("int")
                .IsRequired(false);

            // OrderTypeId - plain int, no FK reference
            builder.Property(m => m.OrderTypeId)
                .HasColumnName("OrderTypeId")
                .HasColumnType("int")
                .IsRequired(false);

            // IncotermId - cross-module FK (Purchase.MiscMaster), no DB constraint
            builder.Property(m => m.IncotermId)
                .HasColumnName("IncotermId")
                .HasColumnType("int")
                .IsRequired(false);

            // PaymentTermsId - cross-module FK (Purchase.PaymentTermMaster), no DB constraint
            builder.Property(m => m.PaymentTermsId)
                .HasColumnName("PaymentTermsId")
                .HasColumnType("int")
                .IsRequired(false);

            // SalesGroupId - cross-module FK (Sales.SalesGroup), no DB constraint
            builder.Property(m => m.SalesGroupId)
                .HasColumnName("SalesGroupId")
                .HasColumnType("int")
                .IsRequired(false);

            // SalesOfficeId - cross-module FK (Sales.SalesOffice), no DB constraint
            builder.Property(m => m.SalesOfficeId)
                .HasColumnName("SalesOfficeId")
                .HasColumnType("int")
                .IsRequired(false);

            // ShippingConditionId - same-module FK to Party.MiscMaster
            builder.Property(m => m.ShippingConditionId)
                .HasColumnName("ShippingConditionId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.HasOne(m => m.ShippingConditionMisc)
                .WithMany(t => t.SalesTypeShippingCondition)
                .HasForeignKey(m => m.ShippingConditionId)
                .OnDelete(DeleteBehavior.Restrict);

            // AccountAssignmentId - same-module FK to Party.MiscMaster
            builder.Property(m => m.AccountAssignmentId)
                .HasColumnName("AccountAssignmentId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.HasOne(m => m.AccountAssignmentMisc)
                .WithMany(t => t.SalesTypeAccountAssignment)
                .HasForeignKey(m => m.AccountAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Active - byte (0/1)
            builder.Property(m => m.Active)
                .HasColumnName("Active")
                .HasColumnType("tinyint")
                .IsRequired();

            // Indexes
            builder.HasIndex(m => m.PartyId);
            builder.HasIndex(m => m.SalesGroupId);
            builder.HasIndex(m => m.SalesOfficeId);
        }
    }
}
