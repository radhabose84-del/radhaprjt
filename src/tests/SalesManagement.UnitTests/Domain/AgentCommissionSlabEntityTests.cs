using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class AgentCommissionSlabEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new AgentCommissionSlab();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new AgentCommissionSlab();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(AgentCommissionSlab)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new AgentCommissionSlab
        {
            Id = 1,
            AgentCommissionConfigId = 10,
            SlabOrder = 2,
            FromDelay = 0,
            ToDelay = 30,
            CommissionTypeId = 5,
            CommissionBasisId = 3,
            CommissionValue = 12.75m
        };

        entity.Id.Should().Be(1);
        entity.AgentCommissionConfigId.Should().Be(10);
        entity.SlabOrder.Should().Be(2);
        entity.FromDelay.Should().Be(0);
        entity.ToDelay.Should().Be(30);
        entity.CommissionTypeId.Should().Be(5);
        entity.CommissionBasisId.Should().Be(3);
        entity.CommissionValue.Should().Be(12.75m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new AgentCommissionSlab
        {
            ToDelay = null,
            AgentCommissionConfig = null,
            CommissionType = null,
            CommissionBasis = null
        };

        entity.ToDelay.Should().BeNull();
        entity.AgentCommissionConfig.Should().BeNull();
        entity.CommissionType.Should().BeNull();
        entity.CommissionBasis.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var config = new AgentCommissionConfig();
        var commissionType = new MiscMaster();
        var commissionBasis = new MiscMaster();

        var entity = new AgentCommissionSlab
        {
            AgentCommissionConfig = config,
            CommissionType = commissionType,
            CommissionBasis = commissionBasis
        };

        entity.AgentCommissionConfig.Should().BeSameAs(config);
        entity.CommissionType.Should().BeSameAs(commissionType);
        entity.CommissionBasis.Should().BeSameAs(commissionBasis);
    }
}
