using FAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Infrastructure.Data.Configurations
{
    public class ManufactureConfiguration : IEntityTypeConfiguration<Manufactures>
    {
        public void Configure(EntityTypeBuilder<Manufactures> builder)
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

            builder.ToTable("Manufacture", "FixedAsset");
            // Primary Key
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
            .HasColumnName("Id")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(dg => dg.Code)                
            .HasColumnType("varchar(10)")
            .IsRequired();    

            builder.Property(dg => dg.ManufactureName)                
            .HasColumnType("varchar(50)")
            .IsRequired();             
    
            builder.Property(dg => dg.ManufactureType)                
            .HasColumnType("int")
            .IsRequired(false);        

            builder.HasOne(dg => dg.ManufactureTypes)
            .WithMany(mm => mm.Manufactures) 
            .HasForeignKey(dg => dg.ManufactureType)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ManufactureType_Misc");        
           
            builder.Property(dg => dg.CountryId)
            .HasColumnType("int")
            .IsRequired();             

            builder.Property(b => b.StateId)                
            .HasColumnType("int")
            .IsRequired();

            builder.Property(b => b.CityId)                
            .HasColumnType("int")
            .IsRequired();

            builder.Property(b => b.AddressLine1)                
            .HasColumnType("varchar(250)")
            .IsRequired();

            builder.Property(b => b.AddressLine2)                
            .HasColumnType("varchar(250)")
            .IsRequired();
            
            builder.Property(b => b.PinCode)                
            .HasColumnType("varchar(10)")
            .IsRequired();

            builder.Property(ag => ag.PersonName)            
            .HasColumnType("varchar(50)")
            .IsRequired(); 

            builder.Property(ag => ag.PhoneNumber)            
            .HasColumnType("varchar(20)")
            .IsRequired(); 

            builder.Property(ag => ag.Email)            
            .HasColumnType("varchar(50)")
            .IsRequired(); 

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
        }
    }
}