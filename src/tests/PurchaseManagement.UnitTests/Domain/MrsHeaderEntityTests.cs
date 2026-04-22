using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.Issue;
using PurchaseManagement.Domain.Entities.MRS;

namespace PurchaseManagement.UnitTests.Domain
{
    public class MrsHeaderEntityTests
    {
        [Fact]
        public void MrsHeader_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MrsHeader)).Should().BeFalse();
        }

        [Fact]
        public void MrsHeader_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new MrsHeader
            {
                Id = 1,
                UnitId = 10,
                RequestCategoryId = 2,
                MrsNo = "MRS001",
                MrsDate = now,
                DepartmentId = 3,
                SubDepartmentId = 4,
                SubStoresWarehouseId = 5,
                CreatedBy = 1,
                CreatedDate = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                ModifiedBy = 2,
                ModifiedDate = now,
                ModifiedByName = "editor",
                ModifiedIP = "192.168.1.1",
                ApprovedBy = 3,
                ApprovedDate = now,
                ApprovedByName = "manager",
                ApprovedIP = "192.168.1.2",
                Remarks = "Test MRS",
                StatusId = 1
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(10);
            entity.MrsNo.Should().Be("MRS001");
            entity.MrsDate.Should().Be(now);
            entity.DepartmentId.Should().Be(3);
            entity.SubDepartmentId.Should().Be(4);
            entity.StatusId.Should().Be(1);
        }

        [Fact]
        public void MrsHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MrsHeader
            {
                MrsNo = null,
                SubStoresWarehouseId = null,
                CreatedDate = null,
                CreatedByName = null,
                CreatedIP = null,
                ModifiedBy = null,
                ModifiedDate = null,
                ModifiedByName = null,
                ModifiedIP = null,
                ApprovedBy = null,
                ApprovedDate = null,
                ApprovedByName = null,
                ApprovedIP = null,
                Remarks = null,
                StatusRequest = null,
                StatusMrs = null,
                MrsDetailHeaderName = null,
                MrsIssueHeaderName = null
            };

            entity.MrsNo.Should().BeNull();
            entity.SubStoresWarehouseId.Should().BeNull();
            entity.ModifiedBy.Should().BeNull();
            entity.ApprovedBy.Should().BeNull();
            entity.MrsDetailHeaderName.Should().BeNull();
        }

        [Fact]
        public void MrsHeader_NavigationProperties_ShouldBeAssignable()
        {
            var statusRequest = new MiscMaster();
            var statusMrs = new MiscMaster();
            var details = new List<MrsDetail> { new MrsDetail() };
            var issues = new List<IssueHeader> { new IssueHeader() };

            var entity = new MrsHeader
            {
                StatusRequest = statusRequest,
                StatusMrs = statusMrs,
                MrsDetailHeaderName = details,
                MrsIssueHeaderName = issues
            };

            entity.StatusRequest.Should().BeSameAs(statusRequest);
            entity.StatusMrs.Should().BeSameAs(statusMrs);
            entity.MrsDetailHeaderName.Should().HaveCount(1);
            entity.MrsIssueHeaderName.Should().HaveCount(1);
        }
    }
}
