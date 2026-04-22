using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Issue;
using PurchaseManagement.Domain.Entities.IssueReturn;
using PurchaseManagement.Domain.Entities.MRS;

namespace PurchaseManagement.UnitTests.Domain
{
    public class IssueHeaderEntityTests
    {
        [Fact]
        public void IssueHeader_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(IssueHeader)).Should().BeFalse();
        }

        [Fact]
        public void IssueHeader_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new IssueHeader
            {
                Id = 1,
                UnitId = 10,
                IssueNo = "ISS001",
                IssueDate = now,
                MrsHeaderId = 5,
                SubStoresWarehouseId = 3,
                IssuedBy = 1,
                IssuedDate = now,
                IssuedByName = "admin",
                IssuedIp = "127.0.0.1",
                Remarks = "Test issue"
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(10);
            entity.IssueNo.Should().Be("ISS001");
            entity.IssueDate.Should().Be(now);
            entity.MrsHeaderId.Should().Be(5);
            entity.IssuedBy.Should().Be(1);
            entity.Remarks.Should().Be("Test issue");
        }

        [Fact]
        public void IssueHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new IssueHeader
            {
                IssueNo = null,
                SubStoresWarehouseId = null,
                IssuedDate = null,
                IssuedByName = null,
                IssuedIp = null,
                Remarks = null,
                IssueHeaderName = null,
                IssueReturnHeaderName = null
            };

            entity.IssueNo.Should().BeNull();
            entity.SubStoresWarehouseId.Should().BeNull();
            entity.IssuedDate.Should().BeNull();
            entity.IssueHeaderName.Should().BeNull();
            entity.IssueReturnHeaderName.Should().BeNull();
        }

        [Fact]
        public void IssueHeader_NavigationProperties_ShouldBeAssignable()
        {
            var mrsHeader = new MrsHeader();
            var details = new List<IssueDetail> { new IssueDetail() };
            var returns = new List<IssueReturnHeader> { new IssueReturnHeader() };

            var entity = new IssueHeader
            {
                MrsHeaderIssueDetails = mrsHeader,
                IssueHeaderName = details,
                IssueReturnHeaderName = returns
            };

            entity.MrsHeaderIssueDetails.Should().BeSameAs(mrsHeader);
            entity.IssueHeaderName.Should().HaveCount(1);
            entity.IssueReturnHeaderName.Should().HaveCount(1);
        }
    }
}
