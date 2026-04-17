using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class CommissionSplitDetailEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new CommissionSplitDetail();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new CommissionSplitDetail();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(CommissionSplitDetail)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new CommissionSplitDetail
        {
            Id = 1,
            CommissionSplitId = 10,
            RoleId = 5,
            ShareTypeId = 2,
            ShareValue = 25.50m
        };

        entity.Id.Should().Be(1);
        entity.CommissionSplitId.Should().Be(10);
        entity.RoleId.Should().Be(5);
        entity.ShareTypeId.Should().Be(2);
        entity.ShareValue.Should().Be(25.50m);
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var split = new CommissionSplit();
        var role = new MiscMaster();
        var shareType = new MiscMaster();

        var entity = new CommissionSplitDetail
        {
            CommissionSplit = split,
            Role = role,
            ShareType = shareType
        };

        entity.CommissionSplit.Should().BeSameAs(split);
        entity.Role.Should().BeSameAs(role);
        entity.ShareType.Should().BeSameAs(shareType);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new CommissionSplitDetail
        {
            CommissionSplit = null,
            Role = null,
            ShareType = null
        };

        entity.CommissionSplit.Should().BeNull();
        entity.Role.Should().BeNull();
        entity.ShareType.Should().BeNull();
    }
}
