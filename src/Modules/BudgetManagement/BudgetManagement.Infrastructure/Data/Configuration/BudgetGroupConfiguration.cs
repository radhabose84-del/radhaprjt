using BudgetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static BudgetManagement.Domain.Common.BaseEntity;
namespace BudgetManagement.Infrastructure.Data.Configuration
{
    public class BudgetGroupConfiguration : IEntityTypeConfiguration<BudgetGroup>
    {
        public void Configure(EntityTypeBuilder<BudgetGroup> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("BudgetGroup", "Budget");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.Name)
                .HasColumnName("Name")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(b => b.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(250)")
                .IsRequired(false);

            builder.Property(b => b.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.DepartmentId)
                .HasColumnName("DepartmentId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.CostCenterId)
                .HasColumnName("CostCenterId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.ParentBudgetGroupId)
                .HasColumnName("ParentBudgetGroupId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(b => b.CurrencyId)
                .HasColumnName("CurrencyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.AllocationRuleId)
                .HasColumnName("AllocationRuleId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(b => b.AllocatedPercentage)
                .HasColumnName("AllocatedPercentage")
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);


            builder.Property(b => b.AllocatedSpindleCost)
                .HasColumnName("AllocatedSpindleCost")
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);


            builder.Property(b => b.IsParent)
                .HasColumnName("IsParent")
                .HasColumnType("bit")
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
                .HasColumnName("CreatedByName")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(b => b.CreatedIP)
                .HasColumnName("CreatedIP")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(b => b.CreatedDate)
                .HasColumnName("CreatedDate")
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(b => b.ModifiedByName)
                .HasColumnName("ModifiedByName")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            builder.Property(b => b.ModifiedIP)
                .HasColumnName("ModifiedIP")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            builder.Property(b => b.ModifiedDate)
                .HasColumnName("ModifiedDate")
                .HasColumnType("datetimeoffset")
                .IsRequired(false);

            builder.Property(b => b.BudgetTypeId)
                .HasColumnName("BudgetTypeId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(b => b.CarryForward)
                .HasColumnName("CarryForward")
                .HasColumnType("bit")
                .IsRequired();

            builder
                .HasOne(b => b.ParentBudgetGroup)
                .WithMany()
                .HasForeignKey(b => b.ParentBudgetGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasIndex(b => new { b.UnitId, b.DepartmentId, b.Name })
                .IsUnique();

            builder
                .HasOne(b => b.AllocationRule)
                .WithMany(m => m.BudgetAllocationRules)
                .HasForeignKey(b => b.AllocationRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(b => b.BudgetType)
                .WithMany(m => m.BudgetTypeGroups)
                .HasForeignKey(b => b.BudgetTypeId)
                .OnDelete(DeleteBehavior.Restrict);
   
        }
    }
}