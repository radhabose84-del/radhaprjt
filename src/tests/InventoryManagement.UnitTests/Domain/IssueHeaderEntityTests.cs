using InventoryManagement.Domain.Entities.Issue;

namespace InventoryManagement.UnitTests.Domain
{
    public class IssueHeaderEntityTests
    {
        [Fact]
        public void IssueHeader_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new IssueHeader
            {
                Id = 1,
                UnitId = 10,
                IssueNo = "ISS-2025-001",
                IssueDate = now,
                MrsHeaderId = 5,
                SubStoresWarehouseId = 3,
                IssuedBy = 1,
                IssuedDate = now,
                IssuedByName = "Test User",
                IssuedIp = "127.0.0.1",
                Remarks = "Test remark"
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(10);
            entity.IssueNo.Should().Be("ISS-2025-001");
            entity.MrsHeaderId.Should().Be(5);
            entity.IssuedBy.Should().Be(1);
            entity.IssuedByName.Should().Be("Test User");
            entity.Remarks.Should().Be("Test remark");
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
                Remarks = null
            };

            entity.IssueNo.Should().BeNull();
            entity.SubStoresWarehouseId.Should().BeNull();
            entity.IssuedDate.Should().BeNull();
            entity.IssuedByName.Should().BeNull();
        }

        [Fact]
        public void IssueHeader_IssueHeaderNameCollection_DefaultsToNull()
        {
            var entity = new IssueHeader();
            entity.IssueHeaderName.Should().BeNull();
        }
    }
}
