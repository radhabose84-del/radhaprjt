using FAM.Domain.Entities;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class BaseEntityAuditFieldsTests
    {
        [Fact]
        public void BaseEntity_AuditFields_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new AssetGroup
            {
                CreatedBy = 1,
                CreatedDate = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                ModifiedBy = 2,
                ModifiedDate = now.AddHours(1),
                ModifiedByName = "editor",
                ModifiedIP = "127.0.0.2"
            };
            entity.CreatedBy.Should().Be(1);
            entity.ModifiedBy.Should().Be(2);
        }

        [Fact]
        public void BaseEntity_NullableAuditFields_ShouldAcceptNull()
        {
            var entity = new AssetGroup
            {
                CreatedDate = null,
                CreatedByName = null,
                CreatedIP = null,
                ModifiedBy = null,
                ModifiedDate = null,
                ModifiedByName = null,
                ModifiedIP = null
            };
            entity.CreatedDate.Should().BeNull();
            entity.ModifiedBy.Should().BeNull();
        }
    }
}
