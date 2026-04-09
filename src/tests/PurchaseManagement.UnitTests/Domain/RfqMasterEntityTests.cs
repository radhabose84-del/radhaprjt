using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class RfqMasterEntityTests
    {
        [Fact]
        public void RfqMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new RfqMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void RfqMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new RfqMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void RfqMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RfqMaster)).Should().BeTrue();
        }

        [Fact]
        public void RfqMaster_Properties_ShouldBeAssignable()
        {
            var entity = new RfqMaster
            {
                Id = 1,
                RfqCode = "RFQ001",
                RfqStatusId = 2,
                InitiationTypeId = 3,
                IndentId = 4,
                UnitId = 5
            };

            entity.Id.Should().Be(1);
            entity.RfqCode.Should().Be("RFQ001");
            entity.RfqStatusId.Should().Be(2);
            entity.InitiationTypeId.Should().Be(3);
            entity.IndentId.Should().Be(4);
            entity.UnitId.Should().Be(5);
        }

        [Fact]
        public void RfqMaster_Collections_ShouldBeInitialized()
        {
            var entity = new RfqMaster();

            entity.Items.Should().NotBeNull();
            entity.Suppliers.Should().NotBeNull();
        }

        [Fact]
        public void RfqMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new RfqMaster
            {
                IndentId = null,
                UnitId = null,
                InitiationTypeId = null
            };

            entity.IndentId.Should().BeNull();
            entity.UnitId.Should().BeNull();
            entity.InitiationTypeId.Should().BeNull();
        }
    }
}
