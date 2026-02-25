using UserManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class PasswordLogConfiguration : IEntityTypeConfiguration<PasswordLog>
   {
      public void Configure(EntityTypeBuilder<PasswordLog> builder)
      {
         builder.ToTable("PasswordLog", "AppSecurity");
         builder.HasKey(x => x.Id);

         builder.Property(p => p.Id)
            .HasColumnName("Id")
            .HasColumnType("int")
            .IsRequired();

         builder.Property(p => p.UserId)
            .HasColumnName("UserId")
            .HasColumnType("int")
            .IsRequired();

         builder.Property(p => p.UserName)
            .HasColumnName("UserName")
            .HasColumnType("varchar(50)")
            .IsRequired();

         builder.Property(p => p.PasswordHash)
            .HasColumnName("PasswordHash")
            .HasColumnType("varchar(255)")
            .IsRequired();

         builder.Property(p => p.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("datetime")
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

         builder.Property(p => p.CreatedIP)
            .HasColumnName("CreatedIP")
            .HasColumnType("varchar(255)")
            .IsRequired();

         builder.HasOne(x => x.User)
             .WithMany(x => x.Passwords)
             .HasForeignKey(x => x.UserId)
             .HasPrincipalKey(x => x.UserId) // ✅ Required to avoid Guid mismatch
             .OnDelete(DeleteBehavior.Restrict);

      }
   }
}