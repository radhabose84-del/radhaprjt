using PurchaseManagement.Domain.Entities.Issue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.Issue
{
    public class IssueDetailConfiguration : IEntityTypeConfiguration<IssueDetail>
    {
        public void Configure(EntityTypeBuilder<IssueDetail> builder)
        {
            builder.ToTable("IssueDetail", "Purchase");
            // Primary Key
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.IssueHeaderId)
                 .HasColumnName("IssueHeaderId")
                 .HasColumnType("int")
                 .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.MrsIssueDetails)
                .WithMany(t => t.IssueHeaderName)
                .HasForeignKey(m => m.IssueHeaderId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); //

            builder.Property(m => m.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int")
                .IsRequired();


            builder.Property(m => m.UomId)
                 .HasColumnName("UomId")
                 .HasColumnType("int")
                 .IsRequired();


            builder.Property(dg => dg.RequestQuantity)
                .HasColumnType("decimal(18,3)")
                .IsRequired()
                .HasDefaultValue(0.000m); // Set default value to 0.00


            builder.Property(m => m.WarehouseStockId)
               .HasColumnName("WarehouseStockId")
               .HasColumnType("int")
               .IsRequired();

            builder.Property(m => m.StorageTypeId)
            .HasColumnName("StorageTypeId")
            .HasColumnType("int")
            .IsRequired();


            builder.Property(m => m.TargetId)
           .HasColumnName("TargetId")
           .HasColumnType("int")
           .IsRequired();


            builder.Property(dg => dg.IssueQuantity)
                .HasColumnType("decimal(18,3)")
                .IsRequired()
                .HasDefaultValue(0.000m); // Set default value to 0.00


            builder.Property(dg => dg.IssueValue)
                .HasColumnType("decimal(18,3)")
                .IsRequired()
                .HasDefaultValue(0.000m); // Set default value to 0.00



            builder.Property(m => m.CostCenterId)
                 .HasColumnName("CostCenterId")
                 .HasColumnType("int")
                 .IsRequired(false);


            builder.Property(m => m.FinanceCode)
                 .HasColumnName("FinanceCode")
                 .HasColumnType("int")
                 .IsRequired(false);

           
               builder.Property(m => m.Sno)
            .HasColumnName("Sno")
            .HasColumnType("int")
            .IsRequired();
                

            
            
        }
    }
}