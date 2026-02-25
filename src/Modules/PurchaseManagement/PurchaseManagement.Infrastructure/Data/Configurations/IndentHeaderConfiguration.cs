using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations
{
    public class IndentHeaderConfiguration : IEntityTypeConfiguration<IndentHeader>
    {
        public void Configure(EntityTypeBuilder<IndentHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
             v => v == Status.Active,
             v => v ? Status.Active : Status.Inactive
         );


            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("IndentHeader", "Purchase");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();


            builder.Property(m => m.IndentNumber)
                .HasColumnName("IndentNumber")
                .HasColumnType("varchar(250)")
                .IsRequired();

            builder.Property(m => m.IndentDate)
                   .HasColumnName("IndentDate")
                   .HasColumnType("date")
                   .IsRequired();


            builder.Property(m => m.IndentTypeId)
                   .HasColumnName("IndentTypeId")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(m => m.UnitId)
                   .HasColumnName("UnitId")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(m => m.Purpose)
        .HasColumnName("Purpose")
        .HasColumnType("varchar(max)")
        .IsRequired();

            builder.Property(m => m.DepartmentId)
               .HasColumnName("DepartmentId")
               .HasColumnType("int")
               .IsRequired();

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

            builder.HasOne(m => m.IndentType)
            .WithMany(t => t.IndentType)
            .HasForeignKey(m => m.IndentTypeId)
            .HasPrincipalKey(t => t.Id)
            .OnDelete(DeleteBehavior.Restrict);
                
                builder.HasOne(ac => ac.Status)
                .WithMany(am => am.StatusHeader)
                .HasForeignKey(ac => ac.StatusId)
                .OnDelete(DeleteBehavior.NoAction);
           
        }
    }
}