using LogisticsManagement.Domain.Common;
using LogisticsManagement.Domain.Entities;
using static LogisticsManagement.Domain.Common.BaseEntity;

namespace LogisticsManagement.UnitTests.Domain
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
                MiscTypeCode = "FREIGHT",
                Description = "Freight Type"
            };

            entity.Id.Should().Be(1);
            entity.MiscTypeCode.Should().Be("FREIGHT");
            entity.Description.Should().Be("Freight Type");
        }

        [Fact]
        public void MiscTypeMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MiscTypeMaster
            {
                MiscTypeCode = null,
                Description = null,
                MiscMasters = null
            };

            entity.MiscTypeCode.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.MiscMasters.Should().BeNull();
        }

        [Fact]
        public void MiscTypeMaster_NavigationCollection_ShouldBeAssignable()
        {
            var miscMasters = new List<MiscMaster>
            {
                new MiscMaster { Id = 1, Code = "CODE001" },
                new MiscMaster { Id = 2, Code = "CODE002" }
            };

            var entity = new MiscTypeMaster
            {
                MiscMasters = miscMasters
            };

            entity.MiscMasters.Should().HaveCount(2);
        }
    }
}
