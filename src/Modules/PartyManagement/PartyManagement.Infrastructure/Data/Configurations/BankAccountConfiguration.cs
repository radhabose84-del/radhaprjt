using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.Infrastructure.Data.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> b)
    {
        // ValueConverter for Status (enum to bit)
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,                    
            v => v ? Status.Active : Status.Inactive    
        );

        // ValueConverter for IsDelete (enum to bit)
        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,                 
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted 
        );

        b.ToTable("BankAccount", schema: "Party");
        b.HasKey(x => x.Id);

        b.Property(x => x.BankId).IsRequired();        
        b.Property(x => x.AccountNumber).HasMaxLength(50).IsRequired();
        b.Property(x => x.AccountHolderName).HasMaxLength(250).IsRequired();        
        b.Property(x => x.BranchId).IsRequired(); 
        b.Property(x => x.IFSCCode).HasMaxLength(11);
        b.Property(x => x.SWIFTCode).HasMaxLength(11);
        b.Property(x => x.IBan).HasMaxLength(34);
        b.Property(x => x.AccountTypeId).IsRequired();

        b.HasOne(m => m.BankAccountType)
                .WithMany(t => t.BankAccountType)
                .HasForeignKey(m => m.AccountTypeId) 
                .OnDelete(DeleteBehavior.Restrict); 

         b.HasOne(m => m.BankAccountMisc)
                .WithMany(t => t.BankAccountBranch)
                .HasForeignKey(m => m.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(m => m.BankAccountOwnerType)
                .WithMany(t => t.BankAccountOwnerType)
                .HasForeignKey(m => m.OwnerTypeId)
                .OnDelete(DeleteBehavior.Restrict);

        // OwnerId is polymorphic (UnitId / PartyId / EmployeeId per OwnerTypeId) — no FK constraint.
        b.Property(x => x.OwnerId).HasColumnType("int");

        // Bank branch address. CityId/StateId are cross-module (UserManagement) — no FK constraint;
        // names resolved via ICityLookup / IStateLookup.
        b.Property(x => x.AddressLine1).HasColumnType("nvarchar(250)");
        b.Property(x => x.AddressLine2).HasColumnType("nvarchar(250)");
        b.Property(x => x.CityId).HasColumnType("int");
        b.Property(x => x.StateId).HasColumnType("int");
        b.Property(x => x.Pincode).HasColumnType("nvarchar(10)");

        b.Property(x => x.IsDefaultAccount).HasDefaultValue(false);
        b.Property(x => x.IsPrimaryAccount).HasDefaultValue(false);
    
        b.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");


        b.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(20)");

        b.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

        b.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(20)");
    }
}