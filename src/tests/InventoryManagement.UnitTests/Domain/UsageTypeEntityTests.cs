using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class UsageTypeEntityTests
    {
        [Fact]
        public void UsageType_DefaultIsActive_ShouldBeActive()
        {
            var entity = new UsageType();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UsageType_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new UsageType();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UsageType_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UsageType)).Should().BeTrue();
        }

        [Fact]
        public void UsageType_Properties_ShouldBeAssignable()
        {
            var entity = new UsageType
            {
                Id = 1,
                UsageTypeCode = "UTY001",
                UsageTypeName = "Test UsageType",
                Description = "Desc",
                ModuleId = 2
            };
            entity.Id.Should().Be(1);
            entity.UsageTypeCode.Should().Be("UTY001");
            entity.UsageTypeName.Should().Be("Test UsageType");
            entity.ModuleId.Should().Be(2);
        }

        [Fact]
        public void UsageType_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UsageType
            {
                UsageTypeCode = null,
                UsageTypeName = null,
                Description = null
            };
            entity.UsageTypeCode.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
