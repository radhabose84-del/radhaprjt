

using InventoryManagement.Domain.Entities.item.ItemDetail.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail.Templates
{
    public sealed class InspectionParameterConfiguration : IEntityTypeConfiguration<InspectionParameter>
    {
        public void Configure(EntityTypeBuilder<InspectionParameter> b)
        {
            b.ToTable("InspectionParameter", "Inventory");
            b.HasKey(x => x.Id);

            b.Property(x => x.Parameter)
             .HasColumnType("varchar(150)")
             .IsRequired();

            b.Property(x => x.AcceptanceCriteriaValue)
             .HasColumnType("varchar(100)");

            b.Property(x => x.Numeric).HasColumnType("bit");

            b.Property(x => x.MinimumValue).HasColumnType("decimal(18,3)");
            b.Property(x => x.MaximumValue).HasColumnType("decimal(18,3)");

            b.HasOne(x => x.Template)
             .WithMany(t => t.Parameters)
             .HasForeignKey(x => x.TemplateId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
