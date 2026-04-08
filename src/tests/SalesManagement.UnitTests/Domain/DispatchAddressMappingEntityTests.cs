using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class DispatchAddressMappingEntityTests
{
    [Fact]
    public void DispatchAddressMapping_DefaultIsActive_ShouldBeActive()
    {
        var entity = new DispatchAddressMapping();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DispatchAddressMapping_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new DispatchAddressMapping();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void DispatchAddressMapping_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(DispatchAddressMapping)).Should().BeTrue();
    }

    [Fact]
    public void DispatchAddressMapping_Properties_ShouldBeAssignable()
    {
        var entity = new DispatchAddressMapping
        {
            Id = 1,
            PartyId = 10,
            DispatchAddressId = 20,
            UsageTypeId = 30,
            IsDefault = true
        };

        entity.Id.Should().Be(1);
        entity.PartyId.Should().Be(10);
        entity.DispatchAddressId.Should().Be(20);
        entity.UsageTypeId.Should().Be(30);
        entity.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void DispatchAddressMapping_NavigationProperties_ShouldBeAssignable()
    {
        var entity = new DispatchAddressMapping
        {
            DispatchAddress = new DispatchAddressMaster(),
            UsageType = new MiscMaster()
        };

        entity.DispatchAddress.Should().NotBeNull();
        entity.UsageType.Should().NotBeNull();
    }

    [Fact]
    public void DispatchAddressMapping_NavigationProperties_ShouldAcceptNull()
    {
        var entity = new DispatchAddressMapping
        {
            DispatchAddress = null,
            UsageType = null
        };

        entity.DispatchAddress.Should().BeNull();
        entity.UsageType.Should().BeNull();
    }
}
