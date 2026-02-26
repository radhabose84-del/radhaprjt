using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class AgentCommissionConfigEntityTests
    {
        [Fact]
        public void AgentCommissionConfig_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AgentCommissionConfig();

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AgentCommissionConfig_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AgentCommissionConfig();

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AgentCommissionConfig_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AgentCommissionConfig)).Should().BeTrue();
        }

        [Fact]
        public void AgentCommissionConfig_Properties_ShouldBeAssignable()
        {
            var from = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var to   = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);

            var entity = new AgentCommissionConfig
            {
                Id = 1,
                AgentId = 10,
                SalesSegmentId = 20,
                ItemId = 30,
                CommissionTypeId = 40,
                CommissionPercentage = 5.5m,
                ValidityFrom = from,
                ValidityTo = to
            };

            entity.Id.Should().Be(1);
            entity.AgentId.Should().Be(10);
            entity.SalesSegmentId.Should().Be(20);
            entity.ItemId.Should().Be(30);
            entity.CommissionTypeId.Should().Be(40);
            entity.CommissionPercentage.Should().Be(5.5m);
            entity.ValidityFrom.Should().Be(from);
            entity.ValidityTo.Should().Be(to);
        }

        [Fact]
        public void AgentCommissionConfig_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AgentCommissionConfig
            {
                UomId = null,
                CurrencyId = null,
                SubAgentPercentage = null
            };

            entity.UomId.Should().BeNull();
            entity.CurrencyId.Should().BeNull();
            entity.SubAgentPercentage.Should().BeNull();
        }

        [Fact]
        public void AgentCommissionConfig_NullableProperties_ShouldAcceptValues()
        {
            var entity = new AgentCommissionConfig
            {
                UomId = 50,
                CurrencyId = 60,
                SubAgentPercentage = 2.0m
            };

            entity.UomId.Should().Be(50);
            entity.CurrencyId.Should().Be(60);
            entity.SubAgentPercentage.Should().Be(2.0m);
        }

        [Fact]
        public void AgentCommissionConfig_IsActive_CanBeSetToInactive()
        {
            var entity = new AgentCommissionConfig
            {
                IsActive = Status.Inactive
            };

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void AgentCommissionConfig_IsDeleted_CanBeSetToDeleted()
        {
            var entity = new AgentCommissionConfig
            {
                IsDeleted = IsDelete.Deleted
            };

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
