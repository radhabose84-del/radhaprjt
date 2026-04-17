using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class AgentCommissionSalesGroupEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new AgentCommissionSalesGroup();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new AgentCommissionSalesGroup();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(AgentCommissionSalesGroup)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new AgentCommissionSalesGroup
        {
            Id = 1,
            AgentCommissionConfigId = 10,
            SalesGroupId = 5
        };

        entity.Id.Should().Be(1);
        entity.AgentCommissionConfigId.Should().Be(10);
        entity.SalesGroupId.Should().Be(5);
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var config = new AgentCommissionConfig();
        var salesGroup = new SalesGroup();

        var entity = new AgentCommissionSalesGroup
        {
            AgentCommissionConfig = config,
            SalesGroup = salesGroup
        };

        entity.AgentCommissionConfig.Should().BeSameAs(config);
        entity.SalesGroup.Should().BeSameAs(salesGroup);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new AgentCommissionSalesGroup
        {
            AgentCommissionConfig = null,
            SalesGroup = null
        };

        entity.AgentCommissionConfig.Should().BeNull();
        entity.SalesGroup.Should().BeNull();
    }
}
