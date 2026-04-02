using InventoryManagement.Domain.Entities.MRS;

namespace InventoryManagement.UnitTests.Domain
{
    public class MrsHeaderEntityTests
    {
        [Fact]
        public void MrsHeader_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new MrsHeader
            {
                Id = 1,
                UnitId = 10,
                RequestCategoryId = 2,
                MrsNo = "MRS-2025-001",
                MrsDate = now,
                DepartmentId = 5,
                SubDepartmentId = 6,
                SubStoresWarehouseId = 3,
                CreatedBy = 1,
                CreatedDate = now,
                CreatedByName = "Test User",
                CreatedIP = "127.0.0.1",
                Remarks = "Test MRS",
                StatusId = 1
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(10);
            entity.MrsNo.Should().Be("MRS-2025-001");
            entity.DepartmentId.Should().Be(5);
            entity.CreatedBy.Should().Be(1);
            entity.CreatedByName.Should().Be("Test User");
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
                Remarks = null
            };

            entity.MrsNo.Should().BeNull();
            entity.SubStoresWarehouseId.Should().BeNull();
            entity.ModifiedBy.Should().BeNull();
            entity.ApprovedBy.Should().BeNull();
        }

        [Fact]
        public void MrsHeader_CollectionProperties_DefaultToNull()
        {
            var entity = new MrsHeader();
            entity.MrsDetailHeaderName.Should().BeNull();
            entity.MrsIssueHeaderName.Should().BeNull();
        }
    }
}
