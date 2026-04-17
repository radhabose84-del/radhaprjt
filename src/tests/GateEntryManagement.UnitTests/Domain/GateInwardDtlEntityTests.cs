using GateEntryManagement.Domain.Entities;

namespace GateEntryManagement.UnitTests.Domain
{
    public class GateInwardDtlEntityTests
    {
        [Fact]
        public void GateInwardDtl_Properties_ShouldBeAssignable()
        {
            var entity = new GateInwardDtl
            {
                Id = 1,
                GateInwardHdrId = 10,
                ReferenceDocTypeId = 2,
                ReferenceDocNo = "PO00123",
                PartyName = "Test Supplier"
            };

            entity.Id.Should().Be(1);
            entity.GateInwardHdrId.Should().Be(10);
            entity.ReferenceDocTypeId.Should().Be(2);
            entity.ReferenceDocNo.Should().Be("PO00123");
            entity.PartyName.Should().Be("Test Supplier");
        }

        [Fact]
        public void GateInwardDtl_NullableProperties_ShouldAcceptNull()
        {
            var entity = new GateInwardDtl
            {
                ReferenceDocNo = null,
                PartyName = null,
                GateInwardHdr = null
            };

            entity.ReferenceDocNo.Should().BeNull();
            entity.PartyName.Should().BeNull();
            entity.GateInwardHdr.Should().BeNull();
        }

        [Fact]
        public void GateInwardDtl_NavigationProperty_ShouldBeAssignable()
        {
            var header = new GateInwardHdr
            {
                Id = 5,
                GateEntryNo = "GE00001"
            };

            var entity = new GateInwardDtl
            {
                Id = 1,
                GateInwardHdrId = 5,
                GateInwardHdr = header
            };

            entity.GateInwardHdr.Should().NotBeNull();
            entity.GateInwardHdr!.Id.Should().Be(5);
            entity.GateInwardHdr.GateEntryNo.Should().Be("GE00001");
        }

        [Fact]
        public void GateInwardDtl_DefaultValues_ShouldBeZeroOrNull()
        {
            var entity = new GateInwardDtl();

            entity.Id.Should().Be(0);
            entity.GateInwardHdrId.Should().Be(0);
            entity.ReferenceDocTypeId.Should().Be(0);
            entity.ReferenceDocNo.Should().BeNull();
            entity.PartyName.Should().BeNull();
            entity.GateInwardHdr.Should().BeNull();
        }
    }
}
