using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Domain
{
    public class AgentConfigEntityTests
    {
        [Fact]
        public void AgentConfig_DoesNotInheritFromBaseEntity()
        {
            typeof(PartyManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(AgentConfig)).Should().BeFalse();
        }

        [Fact]
        public void AgentConfig_Properties_ShouldBeAssignable()
        {
            var entity = new AgentConfig
            {
                Id = 1,
                PartyId = 10,
                SettlementCycleId = 5,
                TdsApplicable = 1,
                TdsCode = "TDS001",
                DefaultCommissionGl = "GL100",
                AgreementStartDate = DateTimeOffset.UtcNow,
                AgreementEndDate = DateTimeOffset.UtcNow.AddYears(1),
                AgentPayableControlGl = "GL200",
                TargetAmount = 50000m,
                TargetPeriod = "Monthly",
                Status = 1
            };

            entity.Id.Should().Be(1);
            entity.PartyId.Should().Be(10);
            entity.SettlementCycleId.Should().Be(5);
            entity.TdsApplicable.Should().Be(1);
            entity.TdsCode.Should().Be("TDS001");
            entity.DefaultCommissionGl.Should().Be("GL100");
            entity.AgentPayableControlGl.Should().Be("GL200");
            entity.TargetAmount.Should().Be(50000m);
            entity.TargetPeriod.Should().Be("Monthly");
            entity.Status.Should().Be(1);
        }

        [Fact]
        public void AgentConfig_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AgentConfig
            {
                SettlementCycleId = null,
                TdsCode = null,
                DefaultCommissionGl = null,
                AgreementStartDate = null,
                AgreementEndDate = null,
                AgentPayableControlGl = null,
                TargetAmount = null,
                TargetPeriod = null
            };

            entity.SettlementCycleId.Should().BeNull();
            entity.TdsCode.Should().BeNull();
            entity.AgreementStartDate.Should().BeNull();
            entity.TargetAmount.Should().BeNull();
        }

        [Fact]
        public void AgentConfig_NavigationProperties_ShouldBeAssignable()
        {
            var party = new PartyMaster { Id = 10, PartyCode = "P001" };
            var entity = new AgentConfig
            {
                Party = party
            };

            entity.Party.Should().NotBeNull();
            entity.Party.Id.Should().Be(10);
        }
    }
}
