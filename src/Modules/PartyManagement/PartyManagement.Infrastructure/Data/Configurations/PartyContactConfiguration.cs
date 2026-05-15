using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class PartyContactConfiguration  : IEntityTypeConfiguration<PartyContact>
    {
        public void Configure(EntityTypeBuilder<PartyContact> builder)
        {
            builder.ToTable("PartyContact", "Party");
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
            builder.HasOne(m => m.PartyContactId)
                .WithMany(t => t.PartyContactTypes)
                .HasForeignKey(m => m.PartyId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.FirstName)
               .HasColumnName("FirstName")
               .HasColumnType("nvarchar(25)")
                .IsRequired();

            builder.Property(m => m.LastName)
               .HasColumnName("LastName")
               .HasColumnType("nvarchar(25)");

            builder.Property(m => m.GenderId)  // Foreign Key column
               .HasColumnName("GenderId")
               .HasColumnType("int");  // Set as int

            // Foreign Key Relationship
            builder.HasOne(m => m.Gender)
                .WithMany(t => t.PartyGender)
                .HasForeignKey(m => m.GenderId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.Designation)
                .HasColumnName("Designation")
                .HasColumnType("nvarchar(50)");

            builder.Property(m => m.EmailID)
                .HasColumnName("EmailID")
                .HasColumnType("nvarchar(100)");

            builder.Property(m => m.AlternateEmailId)
                .HasColumnName("AlternateEmailId")
                .HasColumnType("nvarchar(100)")
                .IsRequired(false);

            builder.Property(m => m.MobileNo)
                .HasColumnName("MobileNo")
                .HasColumnType("nvarchar(50)");

            builder.Property(m => m.AlternateMobileNumber)
                .HasColumnName("AlternateMobileNumber")
                .HasColumnType("nvarchar(50)")
                .IsRequired(false);

            builder.Property(m => m.Phone)
                .HasColumnName("Phone")
                .HasColumnType("nvarchar(50)");


            builder.Property(m => m.PreferredChannelId)  // Foreign Key column
               .HasColumnName("PreferredChannelId")
               .HasColumnType("int");  // Set as int

            // Foreign Key Relationship
            builder.HasOne(m => m.PreferredChannel)
                .WithMany(t => t.ContactPreferredChannel)
                .HasForeignKey(m => m.PreferredChannelId) // Foreign Key property in MiscMaster
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.ContactTypeId)  // Foreign Key column
             .HasColumnName("ContactTypeId")
             .HasColumnType("int");  // Set as int

            // Foreign Key Relationship
            builder.HasOne(m => m.ContactType)
                .WithMany(t => t.PartyContactType)
                .HasForeignKey(m => m.ContactTypeId) // Foreign Key property in MiscMaster
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed
            
            builder.Property(m => m.ContactBy)
                .HasColumnName("ContactBy")
                .HasColumnType("nvarchar(50)");
  
        }
    }
}