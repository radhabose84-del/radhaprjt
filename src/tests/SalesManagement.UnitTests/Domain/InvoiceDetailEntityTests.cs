using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class InvoiceDetailEntityTests
{
    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new InvoiceDetail
        {
            Id = 1,
            InvoiceHeaderId = 10,
            ItemSno = 1,
            ItemId = 5,
            GstPercentage = 18m,
            NoOfBags = 50m,
            BagWeight = 2500m,
            NetWeight = 2450m,
            RatePerKg = 120.50m,
            DiscountValue = 5m,
            FreightValue = 2m,
            CommissionValue = 3m,
            TaxableAmount = 288750m,
            TaxAmount = 51975m,
            TotalAmount = 340725m
        };

        entity.Id.Should().Be(1);
        entity.InvoiceHeaderId.Should().Be(10);
        entity.ItemId.Should().Be(5);
        entity.BagWeight.Should().Be(2500m);
        entity.NetWeight.Should().Be(2450m);
        entity.TotalAmount.Should().Be(340725m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new InvoiceDetail
        {
            HsnCode = null,
            LotId = null,
            PackTypeId = null,
            UOMId = null,
            InvoiceHeader = null
        };

        entity.HsnCode.Should().BeNull();
        entity.LotId.Should().BeNull();
        entity.PackTypeId.Should().BeNull();
        entity.UOMId.Should().BeNull();
        entity.InvoiceHeader.Should().BeNull();
    }
}
