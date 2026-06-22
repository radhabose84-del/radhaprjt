using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class SalesOrderTypeMasterEntityTests
{
    [Fact]
    public void SalesOrderTypeMaster_DefaultIsActive_ShouldBeActive()
    {
        var entity = new SalesOrderTypeMaster();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void SalesOrderTypeMaster_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new SalesOrderTypeMaster();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void SalesOrderTypeMaster_DefaultAllowsDispatch_ShouldBeTrue()
    {
        var entity = new SalesOrderTypeMaster();
        entity.AllowsDispatch.Should().BeTrue();
    }

    [Fact]
    public void SalesOrderTypeMaster_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(SalesOrderTypeMaster)).Should().BeTrue();
    }

    [Fact]
    public void SalesOrderTypeMaster_Properties_ShouldBeAssignable()
    {
        var entity = new SalesOrderTypeMaster
        {
            Id = 1,
            SoTypeId = 10,
            TaxTypeId = 20,
            TypeName = "Normal Sales Order",
            Description = "Standard order type",
            RequiresValidity = true,
            AllowZeroPrice = true,
            MinPrice = 5m,
            MaxPrice = 50m,
            MaxQty = 100m,
            AllowPriceOverride = true,
            OverrideLimitPercent = 10m,
            ApprovalRequired = true,
            CurrencyRequired = true,
            AllowIGST = true,
            CountryMandatory = true,
            DefaultCurrencyId = 3
        };

        entity.Id.Should().Be(1);
        entity.SoTypeId.Should().Be(10);
        entity.TaxTypeId.Should().Be(20);
        entity.TypeName.Should().Be("Normal Sales Order");
        entity.Description.Should().Be("Standard order type");
        entity.RequiresValidity.Should().BeTrue();
        entity.MinPrice.Should().Be(5m);
        entity.MaxPrice.Should().Be(50m);
        entity.MaxQty.Should().Be(100m);
        entity.OverrideLimitPercent.Should().Be(10m);
        entity.DefaultCurrencyId.Should().Be(3);
    }

    [Fact]
    public void SalesOrderTypeMaster_NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesOrderTypeMaster
        {
            TypeName = null,
            Description = null,
            MinPrice = null,
            MaxPrice = null,
            MaxQty = null,
            OverrideLimitPercent = null,
            DefaultCurrencyId = null
        };

        entity.TypeName.Should().BeNull();
        entity.Description.Should().BeNull();
        entity.MinPrice.Should().BeNull();
        entity.MaxPrice.Should().BeNull();
        entity.MaxQty.Should().BeNull();
        entity.OverrideLimitPercent.Should().BeNull();
        entity.DefaultCurrencyId.Should().BeNull();
    }

    [Fact]
    public void SalesOrderTypeMaster_NavigationProperties_ShouldBeAssignable()
    {
        var entity = new SalesOrderTypeMaster
        {
            SoType = new MiscMaster(),
            SalesOrderHeaders = new List<SalesOrderHeader>()
        };

        entity.SoType.Should().NotBeNull();
        entity.SalesOrderHeaders.Should().NotBeNull();
    }
}
