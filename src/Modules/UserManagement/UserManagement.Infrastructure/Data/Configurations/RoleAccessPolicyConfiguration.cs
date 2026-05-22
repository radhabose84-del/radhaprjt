using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class RoleAccessPolicyConfiguration : IEntityTypeConfiguration<RoleAccessPolicy>
    {
        public void Configure(EntityTypeBuilder<RoleAccessPolicy> builder)
        {
            builder.ToTable("RoleAccessPolicy", "AppSecurity");
            builder.HasKey(rap => rap.Id);
            builder.Property(rap => rap.Id).ValueGeneratedOnAdd();

            builder.Property(rap => rap.AccessPolicyId)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(rap => rap.RoleId)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(rap => rap.ValueId)
                .IsRequired()
                .HasColumnType("int");

            builder.HasOne(rap => rap.AccessPolicy)
                .WithMany(ap => ap.RoleAccessPolicies)
                .HasForeignKey(rap => rap.AccessPolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rap => rap.UserRole)
                .WithMany()
                .HasForeignKey(rap => rap.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(rap => new { rap.AccessPolicyId, rap.RoleId, rap.ValueId }).IsUnique();
        }
    }
}
