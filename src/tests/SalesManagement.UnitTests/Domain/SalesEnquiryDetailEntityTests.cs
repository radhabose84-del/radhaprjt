using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class SalesEnquiryDetailEntityTests
{
    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new SalesEnquiryDetail
        {
            Id = 1,
            SalesEnquiryHeaderId = 10,
            ItemId = 5,
            Quantity = 200m,
            ExmillRate = 100.50m,
            TargetPrice = 95.00m,
            Discount = 5.50m
        };

        entity.Id.Should().Be(1);
        entity.SalesEnquiryHeaderId.Should().Be(10);
        entity.ItemId.Should().Be(5);
        entity.Quantity.Should().Be(200m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesEnquiryDetail
        {
            ExmillRate = null,
            TargetPrice = null,
            Discount = null
        };

        entity.ExmillRate.Should().BeNull();
        entity.TargetPrice.Should().BeNull();
        entity.Discount.Should().BeNull();
    }
}
