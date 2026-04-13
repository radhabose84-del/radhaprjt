using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class ImportPOHeaderEntityTests
    {
        [Fact]
        public void ImportPOHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ImportPOHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ImportPOHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ImportPOHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ImportPOHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ImportPOHeader)).Should().BeTrue();
        }

        [Fact]
        public void ImportPOHeader_Properties_ShouldBeAssignable()
        {
            var etd = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
            var eta = new DateTimeOffset(2026, 2, 20, 0, 0, 0, TimeSpan.Zero);
            var entity = new ImportPOHeader
            {
                Id = 1,
                PurchaseOrderId = 5,
                TTExchangeRate = 83.50m,
                IncotermId = 3,
                ShippingPortId = 7,
                DestinationPortId = 8,
                ModeOfTransportId = 1,
                BillOfLadingNumber = "BL12345",
                VesselName = "Test Vessel",
                ContainerNumber = "CNT001",
                ExpectedTimeOfDeparture = etd,
                ExpectedTimeOfArrival = eta,
                LCNumber = "LC001",
                LCAmount = 100000m,
                IsPartialReceiptAllowed = true
            };

            entity.Id.Should().Be(1);
            entity.PurchaseOrderId.Should().Be(5);
            entity.TTExchangeRate.Should().Be(83.50m);
            entity.IncotermId.Should().Be(3);
            entity.BillOfLadingNumber.Should().Be("BL12345");
            entity.ExpectedTimeOfDeparture.Should().Be(etd);
            entity.ExpectedTimeOfArrival.Should().Be(eta);
            entity.LCNumber.Should().Be("LC001");
            entity.IsPartialReceiptAllowed.Should().BeTrue();
        }

        [Fact]
        public void ImportPOHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ImportPOHeader
            {
                TTExchangeRateId = null,
                TTExchangeRate = null,
                ShippingPortId = null,
                DestinationPortId = null,
                ShippingModeId = null,
                CustomsOfficeId = null,
                OriginCountryId = null,
                BillOfLadingNumber = null,
                VesselName = null,
                LCNumber = null,
                LCAmount = null
            };

            entity.TTExchangeRateId.Should().BeNull();
            entity.ShippingPortId.Should().BeNull();
            entity.LCNumber.Should().BeNull();
            entity.LCAmount.Should().BeNull();
        }

        [Fact]
        public void ImportPOHeader_ImportPODetails_ShouldDefaultToEmptyList()
        {
            var entity = new ImportPOHeader();

            entity.ImportPODetails.Should().NotBeNull();
            entity.ImportPODetails.Should().BeEmpty();
        }
    }
}
