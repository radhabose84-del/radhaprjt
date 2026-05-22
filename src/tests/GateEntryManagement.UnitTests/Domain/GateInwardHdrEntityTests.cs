using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Entities;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.UnitTests.Domain
{
    public class GateInwardHdrEntityTests
    {
        [Fact]
        public void GateInwardHdr_DefaultIsActive_ShouldBeActive()
        {
            var entity = new GateInwardHdr();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void GateInwardHdr_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new GateInwardHdr();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void GateInwardHdr_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(GateInwardHdr)).Should().BeTrue();
        }

        [Fact]
        public void GateInwardHdr_Properties_ShouldBeAssignable()
        {
            var entity = new GateInwardHdr
            {
                Id = 1,
                GateEntryNo = "GE00001",
                VehicleMovementRecordId = 5,
                PartyId = 1099,
                GrossWeight = 1000,
                TareWeight = 200,
                NetWeight = 800,
                QAInspectionRequired = true,
                QAStatusId = 3,
                UnitId = 1,
                Remarks = "Test"
            };

            entity.Id.Should().Be(1);
            entity.GateEntryNo.Should().Be("GE00001");
            entity.VehicleMovementRecordId.Should().Be(5);
            entity.PartyId.Should().Be(1099);
            entity.GrossWeight.Should().Be(1000);
            entity.TareWeight.Should().Be(200);
            entity.NetWeight.Should().Be(800);
            entity.QAInspectionRequired.Should().BeTrue();
            entity.QAStatusId.Should().Be(3);
        }

        [Fact]
        public void GateInwardHdr_NullableProperties_ShouldAcceptNull()
        {
            var entity = new GateInwardHdr
            {
                GateEntryNo = null,
                PartyId = null,
                GrossWeight = null,
                TareWeight = null,
                NetWeight = null,
                QAStatusId = null,
                Remarks = null
            };

            entity.GateEntryNo.Should().BeNull();
            entity.PartyId.Should().BeNull();
            entity.GrossWeight.Should().BeNull();
            entity.QAStatusId.Should().BeNull();
        }

        [Fact]
        public void GateInwardHdr_AttachmentProperties_ShouldBeAssignable()
        {
            var entity = new GateInwardHdr
            {
                AttachmentFileName = "abc.pdf",
                AttachmentFilePath = "GateEntry/abc.pdf"
            };

            entity.AttachmentFileName.Should().Be("abc.pdf");
            entity.AttachmentFilePath.Should().Be("GateEntry/abc.pdf");
        }

        [Fact]
        public void GateInwardHdr_ReceivingTypeAndCourierNumber_ShouldBeAssignable()
        {
            var entity = new GateInwardHdr
            {
                ReceivingTypeId = 10,
                CourierNumber = "DTDC-AWB-12345"
            };

            entity.ReceivingTypeId.Should().Be(10);
            entity.CourierNumber.Should().Be("DTDC-AWB-12345");
        }

        [Fact]
        public void GateInwardHdr_ReceivingTypeAndCourierNumber_ShouldAcceptNull()
        {
            var entity = new GateInwardHdr
            {
                ReceivingTypeId = null,
                CourierNumber = null
            };

            entity.ReceivingTypeId.Should().BeNull();
            entity.CourierNumber.Should().BeNull();
        }

        [Fact]
        public void GateInwardHdr_AttachmentProperties_ShouldAcceptNull()
        {
            var entity = new GateInwardHdr
            {
                AttachmentFileName = null,
                AttachmentFilePath = null
            };

            entity.AttachmentFileName.Should().BeNull();
            entity.AttachmentFilePath.Should().BeNull();
        }

        [Fact]
        public void GateInwardHdr_DetailCollection_ShouldBeAssignable()
        {
            var entity = new GateInwardHdr
            {
                GateInwardDetails = new List<GateInwardDtl>
                {
                    new GateInwardDtl { Id = 1, ReferenceDocTypeId = 1, ReferenceDocNo = "PO001" }
                }
            };

            entity.GateInwardDetails.Should().HaveCount(1);
        }
    }
}
