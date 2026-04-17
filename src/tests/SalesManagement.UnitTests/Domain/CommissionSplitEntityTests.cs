using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class CommissionSplitEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new CommissionSplit();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new CommissionSplit();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(CommissionSplit)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new CommissionSplit
        {
            Id = 1,
            SplitCode = "SPL001",
            SplitName = "Test Split"
        };

        entity.Id.Should().Be(1);
        entity.SplitCode.Should().Be("SPL001");
        entity.SplitName.Should().Be("Test Split");
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new CommissionSplit
        {
            SplitCode = null,
            SplitName = null,
            CommissionSplitDetails = null,
            AgentCommissionConfigs = null
        };

        entity.SplitCode.Should().BeNull();
        entity.SplitName.Should().BeNull();
        entity.CommissionSplitDetails.Should().BeNull();
        entity.AgentCommissionConfigs.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var details = new List<CommissionSplitDetail> { new CommissionSplitDetail() };
        var configs = new List<AgentCommissionConfig> { new AgentCommissionConfig() };

        var entity = new CommissionSplit
        {
            CommissionSplitDetails = details,
            AgentCommissionConfigs = configs
        };

        entity.CommissionSplitDetails.Should().HaveCount(1);
        entity.AgentCommissionConfigs.Should().HaveCount(1);
    }
}
