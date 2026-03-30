using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Entities;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.Domain
{
    public class MiscMasterEntityTests
    {
        [Fact]
        public void MiscMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MiscMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MiscMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MiscMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MiscMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MiscMaster)).Should().BeTrue();
        }

        [Fact]
        public void MiscMaster_Properties_ShouldBeAssignable()
        {
            var entity = new MiscMaster
            {
                Id = 1,
                MiscTypeId = 2,
                Code = "MSC001",
                Description = "Test",
                SortOrder = 5
            };

            entity.Id.Should().Be(1);
            entity.MiscTypeId.Should().Be(2);
            entity.Code.Should().Be("MSC001");
            entity.Description.Should().Be("Test");
            entity.SortOrder.Should().Be(5);
        }

        [Fact]
        public void MiscMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MiscMaster
            {
                Code = null,
                Description = null,
                MiscTypeMaster = null
            };

            entity.Code.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.MiscTypeMaster.Should().BeNull();
        }

        [Fact]
        public void MiscMaster_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new MiscMaster
            {
                MiscTypeMaster = new MiscTypeMaster { Id = 1 },
                BudgetRequests = new List<BudgetRequest> { new BudgetRequest { Id = 1 } }
            };

            entity.MiscTypeMaster.Should().NotBeNull();
            entity.BudgetRequests.Should().HaveCount(1);
        }
    }
}
