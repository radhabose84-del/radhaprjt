using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class BaseEntityAuditFieldsTests
    {
        [Fact]
        public void BaseEntity_AuditFields_ShouldBeAssignable()
        {
            var now = DateTime.UtcNow;
            var entity = new User
            {
                CreatedBy = 1,
                CreatedAt = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                ModifiedBy = 2,
                ModifiedAt = now.AddHours(1),
                ModifiedByName = "editor",
                ModifiedIP = "127.0.0.2"
            };

            entity.CreatedBy.Should().Be(1);
            entity.CreatedAt.Should().Be(now);
            entity.CreatedByName.Should().Be("admin");
            entity.CreatedIP.Should().Be("127.0.0.1");
            entity.ModifiedBy.Should().Be(2);
            entity.ModifiedAt.Should().Be(now.AddHours(1));
            entity.ModifiedByName.Should().Be("editor");
            entity.ModifiedIP.Should().Be("127.0.0.2");
        }

        [Fact]
        public void BaseEntity_NullableAuditFields_ShouldAcceptNull()
        {
            var entity = new User
            {
                CreatedByName = null,
                CreatedIP = null,
                ModifiedBy = null,
                ModifiedAt = null,
                ModifiedByName = null,
                ModifiedIP = null
            };

            entity.CreatedByName.Should().BeNull();
            entity.CreatedIP.Should().BeNull();
            entity.ModifiedBy.Should().BeNull();
            entity.ModifiedAt.Should().BeNull();
            entity.ModifiedByName.Should().BeNull();
            entity.ModifiedIP.Should().BeNull();
        }
    }
}
