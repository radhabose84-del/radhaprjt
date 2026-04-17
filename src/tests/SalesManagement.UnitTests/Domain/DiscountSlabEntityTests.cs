using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class DiscountSlabEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new DiscountSlab();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new DiscountSlab();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(DiscountSlab)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new DiscountSlab
        {
            Id = 1,
            DiscountMasterId = 10,
            SlabOrder = 2,
            FromValue = 100.00m,
            ToValue = 500.00m,
            DiscountValue = 10.50m
        };

        entity.Id.Should().Be(1);
        entity.DiscountMasterId.Should().Be(10);
        entity.SlabOrder.Should().Be(2);
        entity.FromValue.Should().Be(100.00m);
        entity.ToValue.Should().Be(500.00m);
        entity.DiscountValue.Should().Be(10.50m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new DiscountSlab
        {
            ToValue = null,
            DiscountMaster = null
        };

        entity.ToValue.Should().BeNull();
        entity.DiscountMaster.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var discountMaster = new DiscountMaster();

        var entity = new DiscountSlab
        {
            DiscountMaster = discountMaster
        };

        entity.DiscountMaster.Should().BeSameAs(discountMaster);
    }
}
