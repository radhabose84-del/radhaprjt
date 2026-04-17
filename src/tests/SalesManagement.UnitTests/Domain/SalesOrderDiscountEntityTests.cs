using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class SalesOrderDiscountEntityTests
{
    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new SalesOrderDiscount
        {
            Id = 1,
            SalesOrderHeaderId = 10,
            DiscountMasterId = 5,
            SlabTypeId = 3,
            PaymentTermId = 7
        };

        entity.Id.Should().Be(1);
        entity.SalesOrderHeaderId.Should().Be(10);
        entity.DiscountMasterId.Should().Be(5);
        entity.SlabTypeId.Should().Be(3);
        entity.PaymentTermId.Should().Be(7);
    }

    [Fact]
    public void NullableNavigationProperties_ShouldAcceptNull()
    {
        var entity = new SalesOrderDiscount
        {
            SalesOrderHeader = null,
            DiscountMaster = null,
            SlabTypeMisc = null
        };

        entity.SalesOrderHeader.Should().BeNull();
        entity.DiscountMaster.Should().BeNull();
        entity.SlabTypeMisc.Should().BeNull();
    }
}
