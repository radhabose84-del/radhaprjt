using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.Issue;
using PurchaseManagement.Domain.Entities.IssueReturn;

namespace PurchaseManagement.UnitTests.Domain
{
    public class IssueReturnHeaderEntityTests
    {
        [Fact]
        public void IssueReturnHeader_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(IssueReturnHeader)).Should().BeFalse();
        }

        [Fact]
        public void IssueReturnHeader_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new IssueReturnHeader
            {
                Id = 1,
                UnitId = 10,
                IssueReturnNo = "IR001",
                IssueReturnDate = now,
                IssueHeaderId = 5,
                DepartmentId = 3,
                CreatedBy = 1,
                CreatedDate = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                ApprovedBy = 2,
                ApprovedDate = now,
                ApprovedByName = "manager",
                ApprovedIP = "192.168.1.1",
                Remarks = "Return approved",
                StatusId = 3,
                RequestCategoryId = 1
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(10);
            entity.IssueReturnNo.Should().Be("IR001");
            entity.IssueReturnDate.Should().Be(now);
            entity.DepartmentId.Should().Be(3);
            entity.StatusId.Should().Be(3);
            entity.RequestCategoryId.Should().Be(1);
        }

        [Fact]
        public void IssueReturnHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new IssueReturnHeader
            {
                IssueReturnNo = null,
                IssueHeaderId = null,
                CreatedDate = null,
                CreatedByName = null,
                CreatedIP = null,
                ApprovedBy = null,
                ApprovedDate = null,
                ApprovedByName = null,
                ApprovedIP = null,
                Remarks = null,
                StatusIssueHeader = null,
                IssueReturnDetailsHeaderName = null
            };

            entity.IssueReturnNo.Should().BeNull();
            entity.IssueHeaderId.Should().BeNull();
            entity.ApprovedBy.Should().BeNull();
            entity.IssueReturnDetailsHeaderName.Should().BeNull();
        }

        [Fact]
        public void IssueReturnHeader_NavigationProperties_ShouldBeAssignable()
        {
            var issueHeader = new IssueHeader();
            var status = new MiscMaster();
            var details = new List<IssueReturnDetail> { new IssueReturnDetail() };

            var entity = new IssueReturnHeader
            {
                IssueHeaderDetails = issueHeader,
                StatusIssueHeader = status,
                IssueReturnDetailsHeaderName = details
            };

            entity.IssueHeaderDetails.Should().BeSameAs(issueHeader);
            entity.StatusIssueHeader.Should().BeSameAs(status);
            entity.IssueReturnDetailsHeaderName.Should().HaveCount(1);
        }
    }
}
