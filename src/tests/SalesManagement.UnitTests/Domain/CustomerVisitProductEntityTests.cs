using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class CustomerVisitProductEntityTests
{
    [Fact]
    public void ShouldNotInheritFromBaseEntity()
    {
        typeof(SalesManagement.Domain.Common.BaseEntity)
            .IsAssignableFrom(typeof(CustomerVisitProduct)).Should().BeFalse();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new CustomerVisitProduct
        {
            Id = 1,
            CustomerVisitId = 10,
            ItemId = 20
        };

        entity.Id.Should().Be(1);
        entity.CustomerVisitId.Should().Be(10);
        entity.ItemId.Should().Be(20);
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var visit = new CustomerVisit();

        var entity = new CustomerVisitProduct
        {
            CustomerVisit = visit
        };

        entity.CustomerVisit.Should().BeSameAs(visit);
    }
}
