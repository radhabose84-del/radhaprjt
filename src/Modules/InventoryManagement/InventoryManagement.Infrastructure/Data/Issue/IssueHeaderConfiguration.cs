using InventoryManagement.Domain.Entities.Issue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Issue
{
    public class IssueHeaderConfiguration : IEntityTypeConfiguration<IssueHeader>
    {
         public void Configure(EntityTypeBuilder<IssueHeader> builder)
        {
            builder.ToTable("IssueHeader", "Inventory");
            // Primary Key
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.UnitId)
                   .HasColumnName("UnitId")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(m => m.IssueNo)
                 .HasColumnName("IssueNo")
                 .HasColumnType("nvarchar(100)")
                 .IsRequired();

            builder.Property(b => b.IssueDate)
                    .HasColumnName("IssueDate")
                    .IsRequired()
                    .HasColumnType("DatetimeOffset");

            builder.Property(m => m.MrsHeaderId)
                 .HasColumnName("MrsHeaderId")
                 .HasColumnType("int")
                 .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.MrsHeaderIssueDetails)
                .WithMany(t => t.MrsIssueHeaderName)
                .HasForeignKey(m => m.MrsHeaderId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict);


            builder.Property(m => m.SubStoresWarehouseId)
                 .HasColumnName("SubStoresWarehouseId")
                 .HasColumnType("int")
                 .IsRequired(false);

            builder.Property(m => m.IssuedBy)
                .HasColumnName("IssuedBy")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.IssueDate)
                    .HasColumnName("IssueDate")
                    .IsRequired()
                    .HasColumnType("DatetimeOffset");

            builder.Property(b => b.IssuedByName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.IssuedIp)
                    .IsRequired()
                    .HasColumnType("varchar(20)");
                    
                    builder.Property(b => b.Remarks)
                    .HasColumnType("nvarchar(250)");  

        }
    }
}