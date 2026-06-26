using UserManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class FinancialYearConfiguration  : IEntityTypeConfiguration<FinancialYear>
    {

  public void Configure(EntityTypeBuilder<FinancialYear> builder)
        {
           var isActiveConverter = new ValueConverter<Status, bool>
               (
                    v => v == Status.Active,  
                    v => v ? Status.Active : Status.Inactive 
                );

                var isDeletedConverter = new ValueConverter<IsDelete, bool>
                (
                 v => v == IsDelete.Deleted,  
                 v => v ? IsDelete.Deleted : IsDelete.NotDeleted 
                );


          builder.ToTable("FinancialYear", "AppData");

            builder.HasKey(u => u.Id);

              builder.Property(u => u.Id)
                        .HasColumnName("Id")
                        .HasColumnType("int")
                        .IsRequired();

             
             builder.Property(u => u.StartYear)
            .HasColumnName("StartYear")
            .HasColumnType("varchar(50)")
            .IsRequired();

             builder.Property(u => u.StartDate)
            .HasColumnName("StartDate")
            .HasColumnType("datetime")
            .IsRequired();

             builder.Property(u => u.EndDate)
            .HasColumnName("EndDate")
            .HasColumnType("datetime")
            .IsRequired();


             builder.Property(u => u.FinYearName)
            .HasColumnName("FinYearName")
            .HasColumnType("varchar(50)")
            .IsRequired();

            // US-GL03-01 (refactor 2026-06-26): FY is GLOBAL (not per-company — period-level
            // CompanyId on Finance.AccountingPeriod scopes consumption). FY carries an Open/Closed
            // lifecycle (StatusId → AppData.MiscMaster) + a transition-year flag.
            builder.Property(u => u.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(u => u.IsTransitionYear)
                .HasColumnName("IsTransitionYear")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            // Same-module FK to AppData.MiscMaster (StatusId → 'FYS' MiscType: OPEN / CLOSED).
            builder.HasOne(u => u.StatusMaster)
                .WithMany(mm => mm.FinancialYearsAsStatus)
                .HasForeignKey(u => u.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(u => u.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(isActiveConverter)
                .IsRequired();

                 builder.Property(u => u.IsDeleted)
            .HasColumnName("IsDeleted")
            .HasColumnType("bit")
            .HasConversion(isDeletedConverter)
            .IsRequired();
            

            builder.Property(u => u.CreatedBy)
            .HasColumnName("CreatedBy")
            .HasColumnType("int");   

            builder.Property(u => u.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("datetime");

            builder.Property(u => u.CreatedByName)
            .HasColumnName("CreatedByName")
            .HasColumnType("varchar(50)");

            builder.Property(u => u.CreatedIP)
            .HasColumnName("CreatedIP")
            .HasColumnType("varchar(25)");

            builder.Property(u => u.ModifiedAt)
            .HasColumnName("ModifiedAt")
            .HasColumnType("datetime");

            builder.Property(u => u.ModifiedBy)
            .HasColumnName("ModifiedBy")
            .HasColumnType("int");

            builder.Property(u => u.ModifiedByName)
            .HasColumnName("ModifiedByName")
            .HasColumnType("varchar(50)");


            builder.Property(u => u.ModifiedIP)
            .HasColumnName("ModifiedIP")
            .HasColumnType("varchar(25)"); 

        }
        
    }
}