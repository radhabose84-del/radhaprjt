using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
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
                Id = 6,
                MiscTypeCode = "MT001",
                Description = "Payment Terms"
            };

            entity.Id.Should().Be(6);
            entity.MiscTypeCode.Should().Be("MT001");
            entity.Description.Should().Be("Payment Terms");
        }

        [Fact]
        public void MiscTypeMaster_Collection_ShouldBeAssignable()
        {
            var entity = new MiscTypeMaster
            {
                MiscMasters = new List<MiscMaster>()
            };

            entity.MiscMasters.Should().NotBeNull();
        }
    }
}
