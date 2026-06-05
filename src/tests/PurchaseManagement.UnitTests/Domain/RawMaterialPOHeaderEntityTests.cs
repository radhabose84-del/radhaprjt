using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class RawMaterialPOHeaderEntityTests
    {
        [Fact]
        public void RawMaterialPOHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new RawMaterialPOHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void RawMaterialPOHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new RawMaterialPOHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void RawMaterialPOHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RawMaterialPOHeader)).Should().BeTrue();
        }

        [Fact]
        public void RawMaterialPOHeader_Properties_ShouldBeAssignable()
        {
            var entity = new RawMaterialPOHeader
            {
                Id = 1,
                UnitId = 2,
                PONumber = "RMPO-2026-0001",
                OcrId = 5,
                ProcurementDocumentTypeId = 3,
                StatusId = 4,
                TaxableTotal = 100m,
                TotalGstAmount = 5m,
                NetTotal = 105m,
                Remarks = "note"
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(2);
            entity.PONumber.Should().Be("RMPO-2026-0001");
            entity.OcrId.Should().Be(5);
            entity.ProcurementDocumentTypeId.Should().Be(3);
            entity.StatusId.Should().Be(4);
            entity.NetTotal.Should().Be(105m);
        }

        [Fact]
        public void RawMaterialPOHeader_DetailsNavigation_ShouldBeAssignable()
        {
            var entity = new RawMaterialPOHeader
            {
                RawMaterialPODetails = new List<RawMaterialPODetail> { new() { Id = 1 } }
            };

            entity.RawMaterialPODetails.Should().HaveCount(1);
        }

        [Fact]
        public void RawMaterialPOHeader_CottonDetailFields_ShouldBeAssignable()
        {
            var passing = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var approvedOn = new DateTimeOffset(2026, 6, 2, 0, 0, 0, TimeSpan.Zero);
            var entity = new RawMaterialPOHeader
            {
                CropYear = "2024-2025",
                ArrivalType = "Spot",
                PassingDate = passing,
                CreditDays = 30,
                CottonApprovedBy = "QA Lead",
                CottonApprovedOn = approvedOn,
                DocumentPath = "RMPO-2026-0001.png"
            };

            entity.CropYear.Should().Be("2024-2025");
            entity.ArrivalType.Should().Be("Spot");
            entity.PassingDate.Should().Be(passing);
            entity.CreditDays.Should().Be(30);
            entity.CottonApprovedBy.Should().Be("QA Lead");
            entity.CottonApprovedOn.Should().Be(approvedOn);
            entity.DocumentPath.Should().Be("RMPO-2026-0001.png");
        }

        [Fact]
        public void RawMaterialPOHeader_NullableCottonFields_ShouldAcceptNull()
        {
            var entity = new RawMaterialPOHeader
            {
                CropYear = null,
                ArrivalType = null,
                PassingDate = null,
                CreditDays = null,
                CottonApprovedBy = null,
                CottonApprovedOn = null,
                DocumentPath = null
            };

            entity.CropYear.Should().BeNull();
            entity.PassingDate.Should().BeNull();
            entity.CreditDays.Should().BeNull();
            entity.DocumentPath.Should().BeNull();
        }
    }
}
