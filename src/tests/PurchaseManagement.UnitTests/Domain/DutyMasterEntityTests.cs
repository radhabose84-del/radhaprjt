using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class DutyMasterEntityTests
    {
        [Fact]
        public void DutyMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new DutyMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DutyMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DutyMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DutyMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DutyMaster)).Should().BeTrue();
        }

        [Fact]
        public void DutyMaster_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new DutyMaster
            {
                Id = 1,
                DutyCode = "DC001",
                TariffNumber = "1234.56.78",
                HsnId = 1,
                DutyCategoryId = 2,
                BasicCustomsDutyPercentage = 10m,
                IGSTPercentage = 18m,
                SocialWelfareSurchargePercentage = 10m,
                EffectiveFrom = now
            };

            entity.Id.Should().Be(1);
            entity.DutyCode.Should().Be("DC001");
            entity.TariffNumber.Should().Be("1234.56.78");
            entity.HsnId.Should().Be(1);
            entity.DutyCategoryId.Should().Be(2);
            entity.BasicCustomsDutyPercentage.Should().Be(10m);
            entity.IGSTPercentage.Should().Be(18m);
            entity.EffectiveFrom.Should().Be(now);
        }

        [Fact]
        public void DutyMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new DutyMaster
            {
                DutyCode = null!,
                TariffNumber = null!,
                EffectiveTo = null,
                AgriInfraDevCessPercentage = null,
                AntiDumpingDutyPercentage = null,
                NotificationNumber = null,
                Remarks = null
            };

            entity.DutyCode.Should().BeNull();
            entity.EffectiveTo.Should().BeNull();
            entity.AgriInfraDevCessPercentage.Should().BeNull();
        }
    }
}
