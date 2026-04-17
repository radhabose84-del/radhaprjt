using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class DiscountPaymentTermEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new DiscountPaymentTerm();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new DiscountPaymentTerm();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(DiscountPaymentTerm)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new DiscountPaymentTerm
        {
            Id = 1,
            DiscountMasterId = 10,
            PaymentTermId = 5
        };

        entity.Id.Should().Be(1);
        entity.DiscountMasterId.Should().Be(10);
        entity.PaymentTermId.Should().Be(5);
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var discountMaster = new DiscountMaster();

        var entity = new DiscountPaymentTerm
        {
            DiscountMaster = discountMaster
        };

        entity.DiscountMaster.Should().BeSameAs(discountMaster);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new DiscountPaymentTerm
        {
            DiscountMaster = null
        };

        entity.DiscountMaster.Should().BeNull();
    }
}
