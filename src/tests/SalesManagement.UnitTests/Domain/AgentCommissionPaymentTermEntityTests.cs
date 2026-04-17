using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class AgentCommissionPaymentTermEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new AgentCommissionPaymentTerm();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new AgentCommissionPaymentTerm();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(AgentCommissionPaymentTerm)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new AgentCommissionPaymentTerm
        {
            Id = 1,
            AgentCommissionConfigId = 10,
            PaymentTermId = 5
        };

        entity.Id.Should().Be(1);
        entity.AgentCommissionConfigId.Should().Be(10);
        entity.PaymentTermId.Should().Be(5);
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var config = new AgentCommissionConfig();

        var entity = new AgentCommissionPaymentTerm
        {
            AgentCommissionConfig = config
        };

        entity.AgentCommissionConfig.Should().BeSameAs(config);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new AgentCommissionPaymentTerm
        {
            AgentCommissionConfig = null
        };

        entity.AgentCommissionConfig.Should().BeNull();
    }
}
