// InventoryManagement.Infrastructure/Data/Configurations/Item/ItemDetail/ItemLogConfiguration.cs
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail
{
    public sealed class ItemLogConfiguration : IEntityTypeConfiguration<ItemLog>
    {
        public void Configure(EntityTypeBuilder<ItemLog> b)
        {
            // Table
            b.ToTable("ItemLogs", schema: "Inventory"); // change schema if you prefer

            // Primary key (bigint identity)
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
             .HasColumnName("Id")
             .HasColumnType("bigint")
             .UseIdentityColumn(); // IDENTITY(1,1)

            // Timestamps
            b.Property(x => x.CreatedDate)
             .HasColumnName("CreatedDate")
             .HasColumnType("datetimeoffset")
             .HasDefaultValueSql("SYSUTCDATETIME()")
             .IsRequired();

            // Entity info
            b.Property(x => x.EntityName)
             .HasColumnName("EntityName")
             .HasColumnType("varchar(128)")
             .IsRequired();

            b.Property(x => x.EntityId)
             .HasColumnName("EntityId")
             .HasColumnType("int")
             .IsRequired();

            // Action (Create/Update/Delete/etc.)
            b.Property(x => x.Action)
             .HasColumnName("Action")
             .HasColumnType("varchar(32)")
             .HasDefaultValue("Update")
             .IsRequired();

            b.Property(x => x.PropertyName)
             .HasColumnName("PropertyName")
             .HasColumnType("varchar(100)")             
             .IsRequired();

            b.Property(x => x.OldValue)
             .HasColumnName("OldValue")
             .HasColumnType("nvarchar(max)")             
             .IsRequired(false);
           
            b.Property(x => x.NewValue)
             .HasColumnName("NewValue")
             .HasColumnType("nvarchar(max)")             
             .IsRequired(false);           
           

            // Actor / context
            b.Property(x => x.CreatedBy)
             .HasColumnName("CreatedBy")
             .HasColumnType("int");

            b.Property(x => x.CreatedByName)
             .HasColumnName("CreatedByName")
             .HasColumnType("varchar(64)");

            // IPv4 fits in 15 chars, IPv6 up to 45
            b.Property(x => x.CreatedIP)
             .HasColumnName("CreatedIP")
             .HasColumnType("varchar(45)");

            b.Property(x => x.CorrelationId)
             .HasColumnName("CorrelationId")
             .HasColumnType("varchar(64)");

            // Useful indexes
            b.HasIndex(x => new { x.EntityName, x.EntityId, x.CreatedDate })
             .HasDatabaseName("IX_ItemLogs_Entity_ThenDate");

            b.HasIndex(x => x.CorrelationId)
             .HasDatabaseName("IX_ItemLogs_CorrelationId");
        }
    }
}
