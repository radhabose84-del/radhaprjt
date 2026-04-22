using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class SalesOrderAmendmentDiscountEntityTests
{
    [Fact]
    public void ShouldNotInheritFromBaseEntity()
    {
        typeof(SalesManagement.Domain.Common.BaseEntity)
            .IsAssignableFrom(typeof(SalesOrderAmendmentDiscount)).Should().BeFalse();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new SalesOrderAmendmentDiscount
        {
            Id = 1,
            SalesOrderAmendmentHeaderId = 10,
            SalesOrderDiscountId = 5,
            DiscountMasterId = 3,
            SlabTypeId = 2,
            PaymentTermId = 7,
            DiscountSlabId = 4,
            DiscountRate = 12.5m,
            TotalDiscountValue = 250.75m
        };

        entity.Id.Should().Be(1);
        entity.SalesOrderAmendmentHeaderId.Should().Be(10);
        entity.SalesOrderDiscountId.Should().Be(5);
        entity.DiscountMasterId.Should().Be(3);
        entity.SlabTypeId.Should().Be(2);
        entity.PaymentTermId.Should().Be(7);
        entity.DiscountSlabId.Should().Be(4);
        entity.DiscountRate.Should().Be(12.5m);
        entity.TotalDiscountValue.Should().Be(250.75m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesOrderAmendmentDiscount
        {
            SalesOrderDiscountId = null,
            DiscountSlabId = null,
            DiscountRate = null,
            TotalDiscountValue = null
        };

        entity.SalesOrderDiscountId.Should().BeNull();
        entity.DiscountSlabId.Should().BeNull();
        entity.DiscountRate.Should().BeNull();
        entity.TotalDiscountValue.Should().BeNull();
    }

    [Fact]
    public void NavigationProperty_ShouldBeAssignable()
    {
        var header = new SalesOrderAmendmentHeader { Id = 10 };
        var entity = new SalesOrderAmendmentDiscount
        {
            SalesOrderAmendmentHeaderId = 10,
            SalesOrderAmendmentHeader = header
        };

        entity.SalesOrderAmendmentHeader.Should().NotBeNull();
        entity.SalesOrderAmendmentHeader!.Id.Should().Be(10);
    }

    [Fact]
    public void NavigationProperty_ShouldAcceptNull()
    {
        var entity = new SalesOrderAmendmentDiscount
        {
            SalesOrderAmendmentHeader = null
        };

        entity.SalesOrderAmendmentHeader.Should().BeNull();
    }
}
