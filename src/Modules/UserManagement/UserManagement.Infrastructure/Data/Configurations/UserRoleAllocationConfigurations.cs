using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Domain.Entities;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class UserRoleAllocationConfigurations: IEntityTypeConfiguration<UserRoleAllocation>
    {
        public void Configure(EntityTypeBuilder<UserRoleAllocation> builder)
        {
            builder.ToTable("UserRoleAllocation", "AppSecurity");

            builder.HasKey(ura => ura.Id)
                .HasName("PK_UserRoleAllocations_Id")
                .IsClustered(true);

            builder.Property(ura => ura.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(ura => ura.UserRoleId)
                .HasColumnName("UserRoleId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ura => ura.UserId)
                .HasColumnName("UserId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(u => u.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired();

            builder.HasOne(ura => ura.UserRole)
                .WithMany(ur => ur.UserRoleAllocations)
                .HasForeignKey(ura => ura.UserRoleId);

            builder.HasOne(ura => ura.User)
                .WithMany(u => u.UserRoleAllocations)
                .HasForeignKey(ura => ura.UserId)
                .HasPrincipalKey(u => u.UserId); // ✅ Required to avoid Guid mismatch;
        }
    }
}