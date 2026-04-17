using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class ComplaintDetailEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new ComplaintDetail();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new ComplaintDetail();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(ComplaintDetail)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new ComplaintDetail
        {
            Id = 1,
            ComplaintHeaderId = 10,
            InvoiceHeaderId = 20,
            InvoiceDate = new DateOnly(2026, 3, 15),
            InvoiceTypeId = 3,
            ItemId = 5,
            NumberOfPacks = 50,
            NetWeight = 2500m,
            InvoiceAmount = 300000m
        };

        entity.Id.Should().Be(1);
        entity.ComplaintHeaderId.Should().Be(10);
        entity.ItemId.Should().Be(5);
        entity.NumberOfPacks.Should().Be(50);
        entity.InvoiceAmount.Should().Be(300000m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new ComplaintDetail
        {
            LotId = null,
            DivisionId = null,
            InvoiceHeader = null,
            InvoiceTypeMisc = null,
            ComplaintHeader = null
        };

        entity.LotId.Should().BeNull();
        entity.DivisionId.Should().BeNull();
        entity.InvoiceHeader.Should().BeNull();
        entity.InvoiceTypeMisc.Should().BeNull();
        entity.ComplaintHeader.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var entity = new ComplaintDetail
        {
            ComplaintDetailNatures = new List<ComplaintDetailNature>
            {
                new() { Id = 1 }
            }
        };

        entity.ComplaintDetailNatures.Should().HaveCount(1);
    }
}
