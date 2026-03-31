using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Entities;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.Domain
{
    public class MiscTypeMasterEntityTests
    {
        [Fact]
        public void MiscTypeMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MiscTypeMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MiscTypeMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MiscTypeMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MiscTypeMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MiscTypeMaster)).Should().BeTrue();
        }

        [Fact]
        public void MiscTypeMaster_Properties_ShouldBeAssignable()
        {
            var entity = new MiscTypeMaster
            {
                Id = 1,
                MiscTypeCode = "MTY001",
                Description = "Test"
            };

            entity.Id.Should().Be(1);
            entity.MiscTypeCode.Should().Be("MTY001");
            entity.Description.Should().Be("Test");
        }

        [Fact]
        public void MiscTypeMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MiscTypeMaster
            {
                MiscTypeCode = null,
                Description = null,
                MiscMaster = null
            };

            entity.MiscTypeCode.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.MiscMaster.Should().BeNull();
        }

        [Fact]
        public void MiscTypeMaster_NavigationProperty_ShouldBeAssignable()
        {
            var entity = new MiscTypeMaster
            {
                MiscMaster = new List<MiscMaster> { new MiscMaster { Id = 1 } }
            };

            entity.MiscMaster.Should().HaveCount(1);
        }
    }
}
