using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class PartyTypeConfiguration : IEntityTypeConfiguration<PartyType>
    {
        public void Configure(EntityTypeBuilder<PartyType> builder)
        {
            builder.ToTable("PartyType", "Party");
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
            builder.HasOne(m => m.Party)
                .WithMany(t => t.PartyTypes)
                .HasForeignKey(m => m.PartyId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed


            builder.Property(m => m.PartyTypeId)  // Foreign Key column
              .HasColumnName("PartyTypeId")
              .HasColumnType("int")  // Set as int
              .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.PartyTypeMisc)
                .WithMany(t => t.PartyTypeGroup)
                .HasForeignKey(m => m.PartyTypeId) // Foreign Key property in Misc
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed
                

             builder.Property(m => m.PartyGroupId)  // Foreign Key column
               .HasColumnName("PartyGroupId")
               .HasColumnType("int")  // Set as int
               .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.PartyGroup)
                .WithMany(t => t.PartyTypeGroups)
                .HasForeignKey(m => m.PartyGroupId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed
            
        }
                
    }
}