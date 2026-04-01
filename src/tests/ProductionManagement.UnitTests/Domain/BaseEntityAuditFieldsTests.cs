using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class BaseEntityAuditFieldsTests
    {
        [Fact]
        public void BaseEntity_AuditFields_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new CertificationMaster
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
            var entity = new CertificationMaster
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
            entity.CreatedByName.Should().BeNull();
            entity.CreatedIP.Should().BeNull();
            entity.ModifiedBy.Should().BeNull();
            entity.ModifiedDate.Should().BeNull();
            entity.ModifiedByName.Should().BeNull();
            entity.ModifiedIP.Should().BeNull();
        }

        [Fact]
        public void BaseEntity_IdProperty_ShouldBeAssignable()
        {
            var entity = new CertificationMaster { Id = 42 };
            entity.Id.Should().Be(42);
        }

        [Fact]
        public void BaseEntity_StatusEnum_ShouldHaveCorrectValues()
        {
            ((int)Status.Inactive).Should().Be(0);
            ((int)Status.Active).Should().Be(1);
        }

        [Fact]
        public void BaseEntity_IsDeleteEnum_ShouldHaveCorrectValues()
        {
            ((int)IsDelete.NotDeleted).Should().Be(0);
            ((int)IsDelete.Deleted).Should().Be(1);
        }
    }
}
