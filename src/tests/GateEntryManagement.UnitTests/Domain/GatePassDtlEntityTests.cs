using GateEntryManagement.Domain.Entities;

namespace GateEntryManagement.UnitTests.Domain
{
    public class GatePassDtlEntityTests
    {
        [Fact]
        public void GatePassDtl_Properties_ShouldBeAssignable()
        {
            var docDate = new DateOnly(2026, 4, 15);
            var entity = new GatePassDtl
            {
                Id = 1,
                GatePassHdrId = 10,
                DocTypeId = 3,
                DocId = 50,
                DocNo = "SO00456",
                PartyName = "Test Customer",
                PartyCode = "CUST001",
                DocDate = docDate,
                TotalQty = 150.5m
            };

            entity.Id.Should().Be(1);
            entity.GatePassHdrId.Should().Be(10);
            entity.DocTypeId.Should().Be(3);
            entity.DocId.Should().Be(50);
            entity.DocNo.Should().Be("SO00456");
            entity.PartyName.Should().Be("Test Customer");
            entity.PartyCode.Should().Be("CUST001");
            entity.DocDate.Should().Be(docDate);
            entity.TotalQty.Should().Be(150.5m);
        }

        [Fact]
        public void GatePassDtl_NullableProperties_ShouldAcceptNull()
        {
            var entity = new GatePassDtl
            {
                DocNo = null,
                PartyName = null,
                PartyCode = null,
                DocDate = null,
                GatePassHdr = null
            };

            entity.DocNo.Should().BeNull();
            entity.PartyName.Should().BeNull();
            entity.PartyCode.Should().BeNull();
            entity.DocDate.Should().BeNull();
            entity.GatePassHdr.Should().BeNull();
        }

        [Fact]
        public void GatePassDtl_NavigationProperty_ShouldBeAssignable()
        {
            var header = new GatePassHdr
            {
                Id = 7,
                GatePassNo = "GP00001"
            };

            var entity = new GatePassDtl
            {
                Id = 1,
                GatePassHdrId = 7,
                GatePassHdr = header
            };

            entity.GatePassHdr.Should().NotBeNull();
            entity.GatePassHdr!.Id.Should().Be(7);
            entity.GatePassHdr.GatePassNo.Should().Be("GP00001");
        }

        [Fact]
        public void GatePassDtl_DefaultValues_ShouldBeZeroOrNull()
        {
            var entity = new GatePassDtl();

            entity.Id.Should().Be(0);
            entity.GatePassHdrId.Should().Be(0);
            entity.DocTypeId.Should().Be(0);
            entity.DocId.Should().Be(0);
            entity.TotalQty.Should().Be(0m);
            entity.DocNo.Should().BeNull();
            entity.PartyName.Should().BeNull();
            entity.PartyCode.Should().BeNull();
            entity.DocDate.Should().BeNull();
            entity.GatePassHdr.Should().BeNull();
        }
    }
}
