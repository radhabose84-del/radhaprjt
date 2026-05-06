using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemMasterEntityTests
    {
        [Fact]
        public void ItemMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ItemMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ItemMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ItemMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ItemMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ItemMaster)).Should().BeTrue();
        }

        [Fact]
        public void ItemMaster_Properties_ShouldBeAssignable()
        {
            var entity = new ItemMaster
            {
                Id = 1,
                ItemCode = "ITEM001",
                ItemName = "Test Item",
                HSNId = 10,
                ItemGroupId = 2,
                ItemCategoryId = 3,
                StockUomId = 4,
                IsStockItem = true,
                MaintainStock = true,
                HasVariants = false,
                IsCapitalItem = false,
                IsHazardous = true,
                OldItemId = 9876
            };

            entity.Id.Should().Be(1);
            entity.ItemCode.Should().Be("ITEM001");
            entity.ItemName.Should().Be("Test Item");
            entity.HSNId.Should().Be(10);
            entity.ItemGroupId.Should().Be(2);
            entity.ItemCategoryId.Should().Be(3);
            entity.StockUomId.Should().Be(4);
            entity.IsStockItem.Should().BeTrue();
            entity.MaintainStock.Should().BeTrue();
            entity.HasVariants.Should().BeFalse();
            entity.IsCapitalItem.Should().BeFalse();
            entity.IsHazardous.Should().BeTrue();
            entity.OldItemId.Should().Be(9876);
        }

        [Fact]
        public void ItemMaster_DefaultIsHazardous_ShouldBeFalse()
        {
            var entity = new ItemMaster();
            entity.IsHazardous.Should().BeFalse();
        }

        [Fact]
        public void ItemMaster_DefaultOldItemId_ShouldBeNull()
        {
            var entity = new ItemMaster();
            entity.OldItemId.Should().BeNull();
        }

        [Fact]
        public void ItemMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemMaster
            {
                HSNId = null,
                ItemGroupId = null,
                ItemCategoryId = null,
                StockUomId = null,
                ItemClassificationId = null,
                Description = null,
                ValidFrom = null,
                XPlantMaterialStatusId = null,
                ParentItemId = null,
                ItemImage = null,
                IssueRuleId = null,
                OriginCountryId = null,
                TariffNumber = null
            };

            entity.HSNId.Should().BeNull();
            entity.ItemGroupId.Should().BeNull();
            entity.ItemCategoryId.Should().BeNull();
            entity.StockUomId.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.ParentItemId.Should().BeNull();
            entity.ItemImage.Should().BeNull();
        }

        [Fact]
        public void ItemMaster_DefaultIsOnSpot_ShouldBeFalse()
        {
            var entity = new ItemMaster();
            entity.IsOnSpot.Should().BeFalse();
        }

        [Fact]
        public void ItemMaster_Collections_ShouldDefaultToEmpty()
        {
            var entity = new ItemMaster();
            entity.ChildItems.Should().NotBeNull().And.BeEmpty();
            entity.VariantValues.Should().NotBeNull().And.BeEmpty();
            entity.Suppliers.Should().NotBeNull().And.BeEmpty();
            entity.Manufacture.Should().NotBeNull().And.BeEmpty();
            entity.ItemUOMs.Should().NotBeNull().And.BeEmpty();
            entity.VariantAttributes.Should().NotBeNull().And.BeEmpty();
            entity.ItemUsageTypeMappings.Should().NotBeNull().And.BeEmpty();
        }
    }
}
