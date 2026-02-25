using FAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static FAM.Domain.Common.BaseEntity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


namespace FAM.Infrastructure.Data.Configurations
{
    public class MiscMasterConfiguration   : IEntityTypeConfiguration<MiscMaster>
    {
       

        public void Configure(EntityTypeBuilder<MiscMaster> builder)
        {
             // ValueConverter for Status (enum to bit)
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,                    // Convert to DB (1 for Active)
                v => v ? Status.Active : Status.Inactive    // Convert to Entity
            );

                // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 // Convert to DB (1 for Deleted)
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted // Convert to Entity
            );
            
            builder.ToTable("MiscMaster", "FixedAsset");
          

        // Primary Key is inherited from BaseEntity (e.g., Id)

         // Properties
       // Properties
         // Properties
            builder.Property(m => m.MiscTypeId)  // Foreign Key column
                .HasColumnName("MiscTypeId")
                .HasColumnType("int")  // Set as int
                .IsRequired();

        builder.Property(m => m.Code)
               .HasColumnName("Code")
               .HasColumnType("nvarchar(50)")  // Set as nvarchar(50)
               .IsRequired();

               
        builder.Property(m => m.Description)
               .HasColumnName("description")
               .HasColumnType("varchar(250)")
               .IsRequired();

        builder.Property(m => m.SortOrder)
               .HasColumnName("sortOrder")  
               .HasColumnType("int")
               .HasDefaultValue(0)
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

       // Foreign Key Relationship
            builder.HasOne(m => m.MiscTypeMaster)
                .WithMany(t => t.MiscMaster)
                .HasForeignKey(m => m.MiscTypeId) // Foreign Key property in MiscMaster
                .HasPrincipalKey(t => t.Id)  // Principal Key in MiscTypeMaster (Id)
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed                          
            
        }
    }
}