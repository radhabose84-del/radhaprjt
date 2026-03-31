using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Entities;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.UnitTests.Domain
{
    public class WarehouseMasterEntityTests
    {
        [Fact]
        public void WarehouseMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new WarehouseMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void WarehouseMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new WarehouseMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void WarehouseMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(WarehouseMaster)).Should().BeTrue();
        }

        [Fact]
        public void WarehouseMaster_Properties_ShouldBeAssignable()
        {
            var entity = new WarehouseMaster
            {
                Id = 1,
                WarehouseCode = "WH001",
                WarehouseName = "Main Warehouse",
                UnitId = 10,
                ParentWarehouseId = null,
                IsGroup = true,
                IsVirtualWarehouse = false,
                WarehouseTypeId = 2,
                DepartmentId = 3,
                StorageTypeId = 4,
                AreaTypeId = 5,
                OperationTypeId = 6,
                CapacityUOMId = 7,
                AccountId = 8,
                ContactPersonName = "John",
                MobileNumber = "1234567890",
                Email = "john@test.com",
                AddressLine1 = "123 St",
                AddressLine2 = "Suite 4",
                CityId = 1,
                StateId = 1,
                CountryId = 1,
                Pincode = "400001",
                IsScrapWarehouse = false,
                IsTransitWarehouse = false,
                MaxCapacity = 500m,
                IsDefaultStockEntry = true
            };

            entity.Id.Should().Be(1);
            entity.WarehouseCode.Should().Be("WH001");
            entity.WarehouseName.Should().Be("Main Warehouse");
            entity.UnitId.Should().Be(10);
            entity.IsGroup.Should().BeTrue();
            entity.DepartmentId.Should().Be(3);
            entity.MaxCapacity.Should().Be(500m);
        }

        [Fact]
        public void WarehouseMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new WarehouseMaster
            {
                ParentWarehouseId = null,
                AccountId = null,
                ContactPersonName = null,
                MobileNumber = null,
                Email = null,
                AddressLine2 = null
            };

            entity.ParentWarehouseId.Should().BeNull();
            entity.AccountId.Should().BeNull();
            entity.ContactPersonName.Should().BeNull();
        }

        [Fact]
        public void WarehouseMaster_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new WarehouseMaster();
            entity.ChildWarehouses.Should().NotBeNull();
            entity.AllowedItemGroups.Should().NotBeNull();
            entity.Racks.Should().NotBeNull();
            entity.Bins.Should().NotBeNull();
            entity.ParentWarehouse.Should().BeNull();
        }
    }
}
