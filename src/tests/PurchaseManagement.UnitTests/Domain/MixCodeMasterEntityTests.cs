using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class MixCodeMasterEntityTests
    {
        [Fact]
        public void MixCodeMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MixCodeMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MixCodeMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MixCodeMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MixCodeMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MixCodeMaster)).Should().BeTrue();
        }

        [Fact]
        public void MixCodeMaster_Properties_ShouldBeAssignable()
        {
            var entity = new MixCodeMaster
            {
                Id = 7,
                MixCode = "MIX001",
                MixCodeDesc = "Cotton mix 60/40"
            };

            entity.Id.Should().Be(7);
            entity.MixCode.Should().Be("MIX001");
            entity.MixCodeDesc.Should().Be("Cotton mix 60/40");
        }
    }
}
