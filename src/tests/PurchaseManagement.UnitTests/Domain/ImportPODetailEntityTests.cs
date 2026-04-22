using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;

namespace PurchaseManagement.UnitTests.Domain
{
    public class ImportPODetailEntityTests
    {
        [Fact]
        public void ImportPODetail_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ImportPODetail)).Should().BeFalse();
        }

        [Fact]
        public void ImportPODetail_ShouldImplementIActivityTracked()
        {
            typeof(IActivityTracked).IsAssignableFrom(typeof(ImportPODetail)).Should().BeTrue();
        }

        [Fact]
        public void ImportPODetail_Properties_ShouldBeAssignable()
        {
            var entity = new ImportPODetail
            {
                Id = 1,
                PurchaseHeaderId = 10,
                IndentId = 5,
                ItemId = 50,
                ItemSno = 1,
                UomId = 3,
                Quantity = 100m,
                UnitPrice = 50m,
                DutyMasterId = 2,
                FreightAmount = 500m,
                InsuranceAmount = 200m,
                CIFValue = 5700m,
                BasicCustomDuty = 570m,
                SocialWelfareSurCharges = 57m,
                IGST = 1134m,
                IGSTPercentage = 18m,
                AgriInfraDevCess = 10m,
                AntiDumpingDuty = 0m,
                SafeguardDuty = 0m,
                HealthEducationCess = 0m,
                OtherCharges = 50m,
                TotalValue = 7521m,
                GRBasedIV = true
            };

            entity.Id.Should().Be(1);
            entity.PurchaseHeaderId.Should().Be(10);
            entity.ItemId.Should().Be(50);
            entity.Quantity.Should().Be(100m);
            entity.UnitPrice.Should().Be(50m);
            entity.IGST.Should().Be(1134m);
            entity.TotalValue.Should().Be(7521m);
            entity.GRBasedIV.Should().BeTrue();
        }

        [Fact]
        public void ImportPODetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ImportPODetail
            {
                IndentId = null,
                DutyMasterId = null,
                dutyMaster = null,
                Header = null,
                FreightAmount = null,
                InsuranceAmount = null,
                CIFValue = null,
                BasicCustomDuty = null,
                SocialWelfareSurCharges = null,
                IGSTPercentage = null,
                AgriInfraDevCess = null,
                AntiDumpingDuty = null,
                SafeguardDuty = null,
                HealthEducationCess = null,
                OtherCharges = null
            };

            entity.IndentId.Should().BeNull();
            entity.DutyMasterId.Should().BeNull();
            entity.dutyMaster.Should().BeNull();
            entity.Header.Should().BeNull();
            entity.FreightAmount.Should().BeNull();
        }

        [Fact]
        public void ImportPODetail_NavigationProperties_ShouldBeAssignable()
        {
            var header = new ImportPOHeader();
            var duty = new DutyMaster();

            var entity = new ImportPODetail
            {
                Header = header,
                dutyMaster = duty
            };

            entity.Header.Should().BeSameAs(header);
            entity.dutyMaster.Should().BeSameAs(duty);
        }
    }
}
