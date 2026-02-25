using PurchaseManagement.Domain.Entities.IssueReturn;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.IssueReturn
{
    public class IssueReturnHeaderConfiguration : IEntityTypeConfiguration<IssueReturnHeader>
    {
        public void Configure(EntityTypeBuilder<IssueReturnHeader> builder)
        {
            builder.ToTable("IssueReturnHeader", "Purchase");
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

            builder.Property(m => m.IssueReturnNo)
                .HasColumnName("IssueReturnNo")
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            builder.Property(b => b.IssueReturnDate)
                    .HasColumnName("IssueReturnDate")
                    .IsRequired()
                    .HasColumnType("DatetimeOffset");


            builder.Property(m => m.IssueHeaderId)
                 .HasColumnName("IssueHeaderId")
                 .HasColumnType("int")
                 .IsRequired(false);

            // Foreign Key Relationship
            builder.HasOne(m => m.IssueHeaderDetails)
                .WithMany(t => t.IssueReturnHeaderName)
                .HasForeignKey(m => m.IssueHeaderId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(m => m.DepartmentId)
                 .HasColumnName("DepartmentId")
                 .HasColumnType("int")
                 .IsRequired();


            builder.Property(m => m.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.CreatedDate)
                    .HasColumnName("CreatedDate")
                    .IsRequired()
                    .HasColumnType("DatetimeOffset");

            builder.Property(b => b.CreatedByName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                    .IsRequired()
                    .HasColumnType("varchar(20)");


            builder.Property(m => m.ApprovedBy)
                .HasColumnName("ApprovedBy");

            builder.Property(b => b.ApprovedDate)
                    .HasColumnName("ApprovedDate")
                    .HasColumnType("DatetimeOffset");

            builder.Property(b => b.ApprovedByName)
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.ApprovedIP)
                    .HasColumnType("varchar(20)");


            builder.Property(b => b.Remarks)
                    .HasColumnType("nvarchar(250)");

            builder.Property(m => m.StatusId)
                 .HasColumnName("StatusId")
                 .HasColumnType("int")
                 .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.StatusIssueHeader)
                .WithMany(t => t.IssueReturnMiscRequestHeader)
                .HasForeignKey(m => m.StatusId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Property(m => m.RequestCategoryId)
                 .HasColumnName("RequestCategoryId")
                 .HasColumnType("int")
                 .IsRequired();
            
           
        }
    }
}