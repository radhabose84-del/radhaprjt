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
                CommissionTypeId = 40,
                CommissionBasisId = 70,
                ApplicableLevelId = 80,
                TriggerEventId = 90,
                CommissionSplitId = 110,
                CommissionPercentage = 5.5m,
                ValidityFrom = from,
                ValidityTo = to
            };

            entity.Id.Should().Be(1);
            entity.AgentId.Should().Be(10);
            entity.CommissionTypeId.Should().Be(40);
            entity.CommissionBasisId.Should().Be(70);
            entity.ApplicableLevelId.Should().Be(80);
            entity.TriggerEventId.Should().Be(90);
            entity.CommissionSplitId.Should().Be(110);
            entity.CommissionPercentage.Should().Be(5.5m);
            entity.ValidityFrom.Should().Be(from);
            entity.ValidityTo.Should().Be(to);
        }

        [Fact]
        public void AgentCommissionConfig_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AgentCommissionConfig
            {
                ValidityTo = null,
                SlabTypeId = null
            };

            entity.ValidityTo.Should().BeNull();
            entity.SlabTypeId.Should().BeNull();
        }

        [Fact]
        public void AgentCommissionConfig_NullableProperties_ShouldAcceptValues()
        {
            var entity = new AgentCommissionConfig
            {
                SlabTypeId = 100
            };

            entity.SlabTypeId.Should().Be(100);
        }

        [Fact]
        public void AgentCommissionConfig_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new AgentCommissionConfig
            {
                AgentCommissionSalesGroups = new List<AgentCommissionSalesGroup>(),
                AgentCommissionPaymentTerms = new List<AgentCommissionPaymentTerm>(),
                AgentCommissionSlabs = new List<AgentCommissionSlab>()
            };

            entity.AgentCommissionSalesGroups.Should().NotBeNull();
            entity.AgentCommissionPaymentTerms.Should().NotBeNull();
            entity.AgentCommissionSlabs.Should().NotBeNull();
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
