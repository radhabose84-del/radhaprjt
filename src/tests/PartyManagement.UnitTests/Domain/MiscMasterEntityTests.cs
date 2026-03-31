using PartyManagement.Domain.Common;
using PartyManagement.Domain.Entities;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.UnitTests.Domain
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
                Code = "MC001",
                Description = "Test Misc",
                MiscTypeId = 2,
                SortOrder = 5
            };
            entity.Id.Should().Be(1);
            entity.Code.Should().Be("MC001");
            entity.MiscTypeId.Should().Be(2);
            entity.SortOrder.Should().Be(5);
        }
    }
}
