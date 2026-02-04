// Infrastructure/Data/Configurations/Quality/QualityInspectionTemplateConfiguration.cs
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail.Templates
{
    public sealed class InspectionTemplateConfiguration : IEntityTypeConfiguration<InspectionTemplate>
    {
        public void Configure(EntityTypeBuilder<InspectionTemplate> b)
        {
            b.ToTable("InspectionTemplate", "Inventory");
            b.HasKey(x => x.Id);

            b.Property(x => x.TemplateName)
             .HasColumnType("varchar(200)")
             .IsRequired();

            b.HasIndex(x => x.TemplateName).IsUnique(false);
        }
    }
}
