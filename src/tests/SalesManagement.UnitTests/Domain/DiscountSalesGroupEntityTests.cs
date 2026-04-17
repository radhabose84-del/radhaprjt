using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class DiscountSalesGroupEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new DiscountSalesGroup();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new DiscountSalesGroup();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(DiscountSalesGroup)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new DiscountSalesGroup
        {
            Id = 1,
            DiscountMasterId = 10,
            SalesGroupId = 5
        };

        entity.Id.Should().Be(1);
        entity.DiscountMasterId.Should().Be(10);
        entity.SalesGroupId.Should().Be(5);
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var discountMaster = new DiscountMaster();
        var salesGroup = new SalesGroup();

        var entity = new DiscountSalesGroup
        {
            DiscountMaster = discountMaster,
            SalesGroup = salesGroup
        };

        entity.DiscountMaster.Should().BeSameAs(discountMaster);
        entity.SalesGroup.Should().BeSameAs(salesGroup);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new DiscountSalesGroup
        {
            DiscountMaster = null,
            SalesGroup = null
        };

        entity.DiscountMaster.Should().BeNull();
        entity.SalesGroup.Should().BeNull();
    }
}
