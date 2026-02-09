using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.Infrastructure.Data.Configuration
{
    public class BudgetAllocationConfiguration : IEntityTypeConfiguration<BudgetAllocation>
    {
        public void Configure(EntityTypeBuilder<BudgetAllocation> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
               v => v == Status.Active,
               v => v ? Status.Active : Status.Inactive
           );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("BudgetAllocation", "Budget");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.FinancialYearId)
               .HasColumnName("FinancialYearId")
               .HasColumnType("int")
               .IsRequired();


            builder.Property(b => b.RequestById)
              .HasColumnName("RequestById")
              .HasColumnType("int")
              .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.RequestMonthYearType)
                .WithMany(t => t.BudgetRequestByType)
                .HasForeignKey(m => m.RequestById) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); //


            builder.Property(b => b.RequestMonthId)
            .HasColumnName("RequestMonthId")
            .HasColumnType("int")
            .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.RequestMonthType)
                .WithMany(t => t.BudgetRequestMonthType)
                .HasForeignKey(m => m.RequestMonthId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); //


            builder.Property(b => b.UnitId)
            .HasColumnName("UnitId")
            .HasColumnType("int")
            .IsRequired();



            builder.Property(b => b.RequestId)
            .HasColumnName("RequestId")
            .HasColumnType("int")
            .IsRequired(false);

            // Foreign Key Relationship
            builder.HasOne(m => m.BudgetRequestType)
                .WithMany(t => t.BudgetAllocationRequestType)
                .HasForeignKey(m => m.RequestId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); //


            builder.Property(b => b.BudgetGroupId)
            .HasColumnName("BudgetGroupId")
            .HasColumnType("int")
            .IsRequired(false);

            // Foreign Key Relationship
            builder.HasOne(m => m.BudgetGroupType)
                .WithMany(t => t.BudgetAllocationGroupType)
                .HasForeignKey(m => m.BudgetGroupId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); //


            builder.Property(b => b.BudgetSubGroupId)
            .HasColumnName("BudgetSubGroupId")
            .HasColumnType("int")
            .IsRequired(false);


            builder.Property(b => b.AllocationTypeId)
            .HasColumnName("AllocationTypeId")
            .HasColumnType("int")
            .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.AllocationRuleType)
                .WithMany(t => t.BudgetAllocationType)
                .HasForeignKey(m => m.AllocationTypeId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); //


            builder.Property(b => b.SpindleCount)
            .HasColumnName("SpindleCount")
            .HasColumnType("int")
            .IsRequired(false);


            builder.Property(b => b.RatePerSpindle)
             .HasColumnName("RatePerSpindle")
             .HasColumnType("decimal(18,2)")
             .IsRequired(false)
             .HasDefaultValue(0.00m); // Set default value to 0.00


            builder.Property(b => b.ApprovedAmount)
             .HasColumnName("ApprovedAmount")
             .HasColumnType("decimal(18,2)")
             .IsRequired();


            builder.Property(b => b.Remarks)
            .HasColumnName("Remarks")
            .HasColumnType("nvarchar(250)")
            .IsRequired(false);

            builder.Property(b => b.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(50)");   

            builder.Property(x => x.RemainingBalance)
               .HasPrecision(18, 2);
       
            
        }
    }
}