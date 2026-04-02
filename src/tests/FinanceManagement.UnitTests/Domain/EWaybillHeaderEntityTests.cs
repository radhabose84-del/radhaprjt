using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class EWaybillHeaderEntityTests
    {
        [Fact]
        public void EWaybillHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new EWaybillHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void EWaybillHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new EWaybillHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void EWaybillHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(EWaybillHeader)).Should().BeTrue();
        }

        [Fact]
        public void EWaybillHeader_Properties_ShouldBeAssignable()
        {
            var entity = new EWaybillHeader
            {
                Id = 1,
                EInvoiceHeaderId = 10,
                UnitId = 5,
                EWBNumber = "EWB123",
                InvoiceNo = "INV001",
                InvoiceDate = new DateOnly(2026, 1, 15),
                InvoiceValue = 50000m,
                SupplyType = "Outward",
                SubSupplyType = "Supply",
                DocumentType = "Tax Invoice",
                TransactionType = 1,
                FromGSTIN = "29AAAAA1234A1Z5",
                FromTradeName = "Seller Corp",
                ToGSTIN = "27BBBBB5678B1Z5",
                ToTradeName = "Buyer Corp",
                TotalValue = 45000m,
                CGST = 2500m,
                SGST = 2500m,
                IGST = 0m,
                Cess = 0m,
                TransporterId = 3,
                TransporterGSTIN = "29TTTTT9999T1Z5",
                TransporterName = "Transport Co",
                TransportMode = "1",
                TransDocNo = "TR001",
                TransDocDate = new DateOnly(2026, 1, 16),
                VehicleNo = "KA01AB1234",
                VehicleType = "R",
                Distance = 150,
                PartyId = 7,
                GeneratedDate = DateTimeOffset.UtcNow,
                ValidUpto = DateTimeOffset.UtcNow.AddDays(3),
                EwbStatus = "Generated",
                ErrorCode = null,
                ErrorMessage = null,
                CancelledDate = null,
                CancelReason = null
            };

            entity.Id.Should().Be(1);
            entity.EInvoiceHeaderId.Should().Be(10);
            entity.EWBNumber.Should().Be("EWB123");
            entity.InvoiceValue.Should().Be(50000m);
            entity.TransportMode.Should().Be("1");
            entity.Distance.Should().Be(150);
            entity.EwbStatus.Should().Be("Generated");
        }

        [Fact]
        public void EWaybillHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new EWaybillHeader
            {
                EInvoiceHeaderId = null,
                EWBNumber = null,
                InvoiceNo = null,
                InvoiceDate = null,
                TransactionType = null,
                TransporterId = null,
                TransDocDate = null,
                Distance = null,
                PartyId = null,
                GeneratedDate = null,
                ValidUpto = null,
                EwbStatus = null,
                ErrorCode = null,
                ErrorMessage = null,
                CancelledDate = null,
                CancelReason = null
            };

            entity.EInvoiceHeaderId.Should().BeNull();
            entity.InvoiceDate.Should().BeNull();
            entity.TransporterId.Should().BeNull();
            entity.PartyId.Should().BeNull();
            entity.CancelledDate.Should().BeNull();
        }

        [Fact]
        public void EWaybillHeader_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new EWaybillHeader
            {
                EInvoiceHeader = new EInvoiceHeader { Id = 1 },
                EWaybillDetails = new List<EWaybillDetail> { new EWaybillDetail { Id = 1 } }
            };

            entity.EInvoiceHeader.Should().NotBeNull();
            entity.EWaybillDetails.Should().HaveCount(1);
        }
    }
}
