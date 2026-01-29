using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Domain.Entities;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSessions>
    {
        public void Configure(EntityTypeBuilder<UserSessions> builder)
        {
            builder.ToTable("UserSessions", "AppSecurity");

            builder.HasKey(ua => ua.Id);        
            
            builder.HasOne<User>()
            .WithMany() // If User has a navigation property for sessions, add .WithMany(u => u.UserSessions)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Property(ua => ua.UserId)
                .HasColumnName("UserId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ua => ua.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(ua => ua.ExpiresAt)
                .HasColumnName("ExpiresAt")
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(u => u.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(
                v => v == 1, // convert byte to bool
                v => v ? (byte)1 : (byte)0 // convert bool to byte
                    )
                .IsRequired();

            builder.Property(ua => ua.JwtId )
                .HasColumnName("JwtId")
                .HasColumnType("varchar(50)");
            
            builder.Property(ua => ua.LastActivity)
                .HasColumnName("LastActivity")
                .HasColumnType("datetime")
                .IsRequired();

             builder.Property(ua => ua.BrowserInfo)
            .HasColumnName("BrowserInfo")
            .HasColumnType("varchar(500)");
        }
    }
    
}

