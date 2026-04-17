using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.IssueReturn;

namespace PurchaseManagement.UnitTests.Domain
{
    public class IssueReturnDetailEntityTests
    {
        [Fact]
        public void IssueReturnDetail_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(IssueReturnDetail)).Should().BeFalse();
        }

        [Fact]
        public void IssueReturnDetail_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new IssueReturnDetail
            {
                Id = 1,
                IssueReturnHeaderId = 10,
                ItemId = 50,
                UomId = 3,
                WarehouseStockId = 5,
                StorageTypeId = 2,
                TargetId = 4,
                ReturnQuantity = 10m,
                ReturnValue = 500m,
                ReasonId = 1,
                Remarks = "Excess stock",
                CreatedBy = 1,
                CreatedDate = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                ApprovedBy = 2,
                ApprovedDate = now,
                ApprovedByName = "manager",
                ApprovedIP = "192.168.1.1",
                StatusId = 3,
                SubStoresDepartmentId = 7
            };

            entity.Id.Should().Be(1);
            entity.IssueReturnHeaderId.Should().Be(10);
            entity.ItemId.Should().Be(50);
            entity.ReturnQuantity.Should().Be(10m);
            entity.ReturnValue.Should().Be(500m);
            entity.ReasonId.Should().Be(1);
            entity.StatusId.Should().Be(3);
            entity.SubStoresDepartmentId.Should().Be(7);
        }

        [Fact]
        public void IssueReturnDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new IssueReturnDetail
            {
                StorageTypeId = null,
                TargetId = null,
                Remarks = null,
                CreatedDate = null,
                CreatedByName = null,
                CreatedIP = null,
                ApprovedBy = null,
                ApprovedDate = null,
                ApprovedByName = null,
                ApprovedIP = null,
                IssueDetailReason = null,
                StatusIssueDetailHeader = null,
                SubStoresDepartmentId = null
            };

            entity.StorageTypeId.Should().BeNull();
            entity.TargetId.Should().BeNull();
            entity.Remarks.Should().BeNull();
            entity.ApprovedBy.Should().BeNull();
            entity.SubStoresDepartmentId.Should().BeNull();
        }

        [Fact]
        public void IssueReturnDetail_NavigationProperties_ShouldBeAssignable()
        {
            var header = new IssueReturnHeader();
            var reason = new MiscMaster();
            var status = new MiscMaster();

            var entity = new IssueReturnDetail
            {
                IssueReturnHeaderDetails = header,
                IssueDetailReason = reason,
                StatusIssueDetailHeader = status
            };

            entity.IssueReturnHeaderDetails.Should().BeSameAs(header);
            entity.IssueDetailReason.Should().BeSameAs(reason);
            entity.StatusIssueDetailHeader.Should().BeSameAs(status);
        }
    }
}
