using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class PartyUnitCompanyMappingConfiguration : IEntityTypeConfiguration<PartyUnitCompanyMapping>
    {
        public void Configure(EntityTypeBuilder<PartyUnitCompanyMapping> builder)
        {
            builder.ToTable("PartyUnitCompanyMapping", "Party");
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
            builder.HasOne(m => m.PartyUnitCompany)
                .WithMany(t => t.PartyUnitCompanyMappings)
                .HasForeignKey(m => m.PartyId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.CompanyId)  // Foreign Key column
              .HasColumnName("CompanyId")
              .HasColumnType("int")  // Set as int
              .IsRequired();


             builder.Property(m => m.UnitId)  // Foreign Key column
              .HasColumnName("UnitId")
              .HasColumnType("int")  // Set as int
              .IsRequired();

          
          
            
        }
    }
}