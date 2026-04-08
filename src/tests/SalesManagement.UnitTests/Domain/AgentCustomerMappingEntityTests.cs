using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class AgentCustomerMappingEntityTests
    {
        [Fact]
        public void AgentCustomerMapping_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AgentCustomerMapping();

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AgentCustomerMapping_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AgentCustomerMapping();

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AgentCustomerMapping_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AgentCustomerMapping)).Should().BeTrue();
        }

        [Fact]
        public void AgentCustomerMapping_Properties_ShouldBeAssignable()
        {
            var effectiveFrom = DateTime.Today.AddDays(-30);
            var effectiveTo = DateTime.Today.AddDays(30);

            var entity = new AgentCustomerMapping
            {
                Id = 1,
                CustomerId = 10,
                AgentId = 20,
                SubAgentId = 30,
                SalesSegmentId = 40,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = effectiveTo,
                IsDefaultAgent = true,
                Remarks = "Test remarks"
            };

            entity.Id.Should().Be(1);
            entity.CustomerId.Should().Be(10);
            entity.AgentId.Should().Be(20);
            entity.SubAgentId.Should().Be(30);
            entity.SalesSegmentId.Should().Be(40);
            entity.EffectiveFrom.Should().Be(effectiveFrom);
            entity.EffectiveTo.Should().Be(effectiveTo);
            entity.IsDefaultAgent.Should().BeTrue();
            entity.Remarks.Should().Be("Test remarks");
        }

        [Fact]
        public void AgentCustomerMapping_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AgentCustomerMapping
            {
                SubAgentId = null,
                EffectiveTo = null,
                Remarks = null
            };

            entity.SubAgentId.Should().BeNull();
            entity.EffectiveTo.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }

        [Fact]
        public void AgentCustomerMapping_NullableProperties_ShouldAcceptValues()
        {
            var entity = new AgentCustomerMapping
            {
                SubAgentId = 50,
                EffectiveTo = DateTime.Today.AddDays(60),
                Remarks = "Some remark"
            };

            entity.SubAgentId.Should().Be(50);
            entity.EffectiveTo.Should().NotBeNull();
            entity.Remarks.Should().Be("Some remark");
        }

        [Fact]
        public void AgentCustomerMapping_NavigationProperty_ShouldBeAssignable()
        {
            var segment = new SalesSegment { Id = 1 };
            var entity = new AgentCustomerMapping
            {
                SalesSegment = segment
            };

            entity.SalesSegment.Should().NotBeNull();
            entity.SalesSegment!.Id.Should().Be(1);
        }

        [Fact]
        public void AgentCustomerMapping_IsActive_CanBeSetToInactive()
        {
            var entity = new AgentCustomerMapping
            {
                IsActive = Status.Inactive
            };

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void AgentCustomerMapping_IsDeleted_CanBeSetToDeleted()
        {
            var entity = new AgentCustomerMapping
            {
                IsDeleted = IsDelete.Deleted
            };

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
