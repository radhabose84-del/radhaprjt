using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain
{
    public class OfficerAgentEntityTests
    {
        [Fact]
        public void OfficerAgent_DefaultIsActive_ShouldBeTrue()
        {
            var entity = new OfficerAgent();
            entity.IsActive.Should().BeTrue();
        }

        [Fact]
        public void OfficerAgent_DoesNotInheritFromBaseEntity()
        {
            // OfficerAgent does NOT extend BaseEntity — it has its own audit fields
            typeof(SalesManagement.Domain.Common.BaseEntity).IsAssignableFrom(typeof(OfficerAgent)).Should().BeFalse();
        }

        [Fact]
        public void OfficerAgent_Properties_ShouldBeAssignable()
        {
            var entity = new OfficerAgent
            {
                Id = 1,
                AgentId = 10,
                MarketingOfficerId = 20,
                ValidityFrom = new DateOnly(2026, 1, 1),
                ValidityTo = new DateOnly(2026, 12, 31),
                IsActive = true
            };

            entity.Id.Should().Be(1);
            entity.AgentId.Should().Be(10);
            entity.MarketingOfficerId.Should().Be(20);
            entity.ValidityFrom.Should().Be(new DateOnly(2026, 1, 1));
            entity.ValidityTo.Should().Be(new DateOnly(2026, 12, 31));
            entity.IsActive.Should().BeTrue();
        }

        [Fact]
        public void OfficerAgent_AuditFields_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new OfficerAgent
            {
                CreatedBy = 1,
                CreatedDate = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                ModifiedBy = 2,
                ModifiedDate = now.AddHours(1),
                ModifiedByName = "editor",
                ModifiedIP = "192.168.1.1"
            };

            entity.CreatedBy.Should().Be(1);
            entity.CreatedDate.Should().Be(now);
            entity.CreatedByName.Should().Be("admin");
            entity.CreatedIP.Should().Be("127.0.0.1");
            entity.ModifiedBy.Should().Be(2);
            entity.ModifiedDate.Should().Be(now.AddHours(1));
            entity.ModifiedByName.Should().Be("editor");
            entity.ModifiedIP.Should().Be("192.168.1.1");
        }

        [Fact]
        public void OfficerAgent_NullableAuditFields_ShouldAcceptNull()
        {
            var entity = new OfficerAgent
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

        [Fact]
        public void OfficerAgent_NavigationProperty_ShouldBeAssignable()
        {
            var entity = new OfficerAgent
            {
                MarketingOfficer = new MarketingOfficer()
            };

            entity.MarketingOfficer.Should().NotBeNull();
        }
    }
}
