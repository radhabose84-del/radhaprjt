using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Entities;

namespace GateEntryManagement.UnitTests.Domain
{
    public class BaseEntityAuditFieldsTests
    {
        [Fact]
        public void BaseEntity_AuditFields_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new MiscMaster
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
            entity.CreatedDate.Should().Be(now);
            entity.CreatedByName.Should().Be("admin");
            entity.CreatedIP.Should().Be("127.0.0.1");
            entity.ModifiedBy.Should().Be(2);
            entity.ModifiedDate.Should().Be(now.AddHours(1));
            entity.ModifiedByName.Should().Be("editor");
            entity.ModifiedIP.Should().Be("127.0.0.2");
        }

        [Fact]
        public void BaseEntity_NullableAuditFields_ShouldAcceptNull()
        {
            var entity = new MiscMaster
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
            entity.ModifiedDate.Should().BeNull();
        }
    }
}
