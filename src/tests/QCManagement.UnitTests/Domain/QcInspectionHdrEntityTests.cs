using QCManagement.Domain.Common;
using QCManagement.Domain.Entities;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.UnitTests.Domain
{
    public class QcInspectionHdrEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive()
        {
            new QcInspectionHdr().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new QcInspectionHdr().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(QcInspectionHdr)).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeActivityTracked()
        {
            typeof(IActivityTracked).IsAssignableFrom(typeof(QcInspectionHdr)).Should().BeTrue();
        }

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new QcInspectionHdr
            {
                Id = 1,
                QcInspectionNo = "QCI-2026-00001",
                SourceTypeId = 60,
                SourceHeaderId = 100,
                SourceDetailId = 4321,
                ReceivedQuantity = 1000m,
                ReceivedUomId = 3,
                BatchNumber = "BN-1"
            };

            entity.Id.Should().Be(1);
            entity.QcInspectionNo.Should().Be("QCI-2026-00001");
            entity.SourceTypeId.Should().Be(60);
            entity.SourceHeaderId.Should().Be(100);
            entity.SourceDetailId.Should().Be(4321);
            entity.ReceivedQuantity.Should().Be(1000m);
        }

        [Fact]
        public void DispositionFields_ShouldAcceptNull()
        {
            var entity = new QcInspectionHdr
            {
                QcStatusId = null,
                AcceptedQuantity = null,
                RejectedQuantity = null,
                DispositionDate = null
            };

            entity.QcStatusId.Should().BeNull();
            entity.AcceptedQuantity.Should().BeNull();
        }

        [Fact]
        public void Details_NavigationProperty_ShouldBeAssignable()
        {
            var entity = new QcInspectionHdr
            {
                Details = new List<QcInspectionDtl> { new() { Id = 1 } }
            };

            entity.Details.Should().HaveCount(1);
        }
    }
}
