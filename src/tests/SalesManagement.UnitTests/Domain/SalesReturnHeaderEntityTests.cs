using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class SalesReturnHeaderEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new SalesReturnHeader();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new SalesReturnHeader();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(SalesReturnHeader)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new SalesReturnHeader
        {
            Id = 1,
            ReturnNumber = "SR001",
            ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ComplaintHeaderId = 10,
            CustomerId = 20,
            WarehouseId = 5,
            BinId = 3,
            StatusId = 1,
            Remarks = "Test return"
        };

        entity.Id.Should().Be(1);
        entity.ReturnNumber.Should().Be("SR001");
        entity.ComplaintHeaderId.Should().Be(10);
        entity.CustomerId.Should().Be(20);
        entity.WarehouseId.Should().Be(5);
        entity.BinId.Should().Be(3);
        entity.StatusId.Should().Be(1);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesReturnHeader
        {
            ReturnNumber = null,
            Remarks = null
        };

        entity.ReturnNumber.Should().BeNull();
        entity.Remarks.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var entity = new SalesReturnHeader
        {
            SalesReturnDetails = new List<SalesReturnDetail>
            {
                new() { Id = 1, ItemId = 10 }
            }
        };

        entity.SalesReturnDetails.Should().HaveCount(1);
    }
}
