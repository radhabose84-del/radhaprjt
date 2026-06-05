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
    }
}
