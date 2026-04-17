using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class SalesQuotationDetailEntityTests
{
    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new SalesQuotationDetail
        {
            Id = 1,
            SalesQuotationHeaderId = 10,
            ItemId = 5,
            Quantity = 100m,
            ExMillRate = 120.50m,
            Discount = 5.0m,
            NetRate = 115.50m,
            TotalAmount = 11550m,
            HSNId = 3,
            TaxPercentage = 18m,
            TaxAmount = 2079m
        };

        entity.Id.Should().Be(1);
        entity.SalesQuotationHeaderId.Should().Be(10);
        entity.ItemId.Should().Be(5);
        entity.Quantity.Should().Be(100m);
        entity.TotalAmount.Should().Be(11550m);
    }
}
