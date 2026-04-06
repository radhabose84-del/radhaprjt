using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class EWaybillDetailEntityTests
    {
        [Fact]
        public void EWaybillDetail_Properties_ShouldBeAssignable()
        {
            var entity = new EWaybillDetail
            {
                Id = 1,
                EWaybillHeaderId = 10,
                ItemSno = 1,
                ItemId = 100,
                ItemName = "Test Item",
                HsnNo = "1234",
                IsService = "N",
                Qty = 50m,
                UOM = "KGS",
                TaxableValue = 5000m,
                TaxRate = 18m,
                CGST = 450m,
                SGST = 450m,
                IGST = 0m,
                Cess = 0m,
                IsActive = true,
                IsDeleted = false
            };

            entity.Id.Should().Be(1);
            entity.EWaybillHeaderId.Should().Be(10);
            entity.ItemSno.Should().Be(1);
            entity.ItemId.Should().Be(100);
            entity.ItemName.Should().Be("Test Item");
            entity.HsnNo.Should().Be("1234");
            entity.IsService.Should().Be("N");
            entity.Qty.Should().Be(50m);
            entity.UOM.Should().Be("KGS");
            entity.TaxableValue.Should().Be(5000m);
            entity.TaxRate.Should().Be(18m);
            entity.CGST.Should().Be(450m);
            entity.SGST.Should().Be(450m);
            entity.IGST.Should().Be(0m);
            entity.Cess.Should().Be(0m);
            entity.IsActive.Should().BeTrue();
            entity.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void EWaybillDetail_DefaultIsActive_ShouldBeTrue()
        {
            var entity = new EWaybillDetail();
            entity.IsActive.Should().BeTrue();
        }

        [Fact]
        public void EWaybillDetail_DefaultIsDeleted_ShouldBeFalse()
        {
            var entity = new EWaybillDetail();
            entity.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void EWaybillDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new EWaybillDetail
            {
                ItemName = null,
                HsnNo = null,
                IsService = null,
                UOM = null
            };

            entity.ItemName.Should().BeNull();
            entity.HsnNo.Should().BeNull();
            entity.IsService.Should().BeNull();
            entity.UOM.Should().BeNull();
        }

        [Fact]
        public void EWaybillDetail_NavigationProperty_ShouldBeAssignable()
        {
            var header = new EWaybillHeader { Id = 10 };
            var entity = new EWaybillDetail { EWaybillHeader = header };

            entity.EWaybillHeader.Should().NotBeNull();
            entity.EWaybillHeader!.Id.Should().Be(10);
        }

        [Fact]
        public void EWaybillDetail_NavigationProperty_ShouldAcceptNull()
        {
            var entity = new EWaybillDetail { EWaybillHeader = null };
            entity.EWaybillHeader.Should().BeNull();
        }
    }
}
