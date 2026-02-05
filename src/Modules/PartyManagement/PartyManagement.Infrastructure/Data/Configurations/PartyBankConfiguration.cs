using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class PartyBankConfiguration : IEntityTypeConfiguration<PartyBank>
    {
        public void Configure(EntityTypeBuilder<PartyBank> builder)
        {
            builder.ToTable("PartyBank", "Party");
            // Primary Key
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.PartyId)  // Foreign Key column
               .HasColumnName("PartyId")
               .HasColumnType("int")  // Set as int
               .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.PartyBankId)
                .WithMany(t => t.PartyBankTypes)
                .HasForeignKey(m => m.PartyId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.BankName)
               .HasColumnName("BankName")
               .HasColumnType("nvarchar(50)");

            builder.Property(m => m.BankAccountNumber)
              .HasColumnName("BankAccountNumber")
              .HasColumnType("nvarchar(25)");

            builder.Property(m => m.BankBranch)
               .HasColumnName("BankBranch")
               .HasColumnType("nvarchar(25)");

            builder.Property(m => m.IFSCCode)
               .HasColumnName("IFSCCode")
               .HasColumnType("nvarchar(25)");

            builder.Property(m => m.SWIFTCode)
               .HasColumnName("SWIFTCode")
               .HasColumnType("nvarchar(25)");

            builder.Property(m => m.AccountTypeId)  // Foreign Key column
              .HasColumnName("AccountTypeId")
              .HasColumnType("int");  // Set as int

            // Foreign Key Relationship
            builder.HasOne(m => m.BankAccountType)
                .WithMany(t => t.PartyBankType)
                .HasForeignKey(m => m.AccountTypeId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(t => t.IsDefaultAccount)
                .HasColumnName("IsDefaultAccount")
                .HasColumnType("bit")
                .IsRequired();
                
            builder.Property(t => t.IsPrimaryAccount)
                .HasColumnName("IsPrimaryAccount")
                .HasColumnType("bit")
                .IsRequired();                              

        }
    }
}