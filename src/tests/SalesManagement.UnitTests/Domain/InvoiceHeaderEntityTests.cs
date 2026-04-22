using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class InvoiceHeaderEntityTests
    {
        [Fact]
        public void InvoiceHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new InvoiceHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void InvoiceHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new InvoiceHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void InvoiceHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(InvoiceHeader)).Should().BeTrue();
        }

        [Fact]
        public void InvoiceHeader_Properties_ShouldBeAssignable()
        {
            var entity = new InvoiceHeader
            {
                Id = 1,
                InvoiceNo = "INV001",
                InvoiceDate = new DateOnly(2026, 1, 1),
                DispatchAdviceId = 2,
                PartyId = 3,
                AgentId = 4,
                UnitId = 5,
                FinancialYearId = 6,
                InvoiceTypeId = 9,
                TransportMode = 7,
                StatusId = 8,
                VehicleNumber = "KA01AB1234",
                TransporterName = "ABC Transport",
                LRNumber = "LR001",
                LRDate = new DateOnly(2026, 1, 2),
                TotalBags = 100,
                TotalWeight = 5000m,
                TaxableValue = 10000m,
                TotalDiscount = 500m,
                TotalFreight = 200m,
                TotalCommission = 150m,
                Insurance = 100m,
                HandlingCharge = 50m,
                OtherCharges = 25m,
                CGST = 900m,
                SGST = 900m,
                IGST = 0m,
                TaxAmount = 1800m,
                TCSPercentage = 0.1m,
                TCS = 10m,
                RoundOff = 0.50m,
                InvoiceAmountBeforeTCS = 11675m,
                InvoiceAmount = 11685m,
                Remarks = "Test",
                GEFlag = false
            };

            entity.Id.Should().Be(1);
            entity.InvoiceNo.Should().Be("INV001");
            entity.DispatchAdviceId.Should().Be(2);
            entity.TotalBags.Should().Be(100);
            entity.InvoiceAmount.Should().Be(11685m);
            entity.GEFlag.Should().BeFalse();
        }

        [Fact]
        public void InvoiceHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new InvoiceHeader
            {
                AgentId = null,
                TransportMode = null,
                StatusId = null,
                LRDate = null,
                VehicleNumber = null,
                TransporterName = null,
                LRNumber = null,
                Remarks = null
            };

            entity.AgentId.Should().BeNull();
            entity.TransportMode.Should().BeNull();
            entity.StatusId.Should().BeNull();
            entity.LRDate.Should().BeNull();
        }

        [Fact]
        public void InvoiceHeader_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new InvoiceHeader
            {
                InvoiceDetails = new List<InvoiceDetail>(),
                DispatchAdviceHeader = new DispatchAdviceHeader { Id = 1 }
            };

            entity.InvoiceDetails.Should().NotBeNull();
            entity.DispatchAdviceHeader.Should().NotBeNull();
        }
    }
}
