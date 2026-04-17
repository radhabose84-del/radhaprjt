using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class SalesReturnDetailEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new SalesReturnDetail();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new SalesReturnDetail();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(SalesReturnDetail)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new SalesReturnDetail
        {
            Id = 1,
            SalesReturnHeaderId = 10,
            InvoiceHeaderId = 20,
            InvoiceDetailId = 30,
            ItemId = 5,
            StartPackNo = 1,
            EndPackNo = 50,
            ReturnQty = 500m,
            BagStatusId = 2
        };

        entity.Id.Should().Be(1);
        entity.SalesReturnHeaderId.Should().Be(10);
        entity.ItemId.Should().Be(5);
        entity.ReturnQty.Should().Be(500m);
        entity.BagStatusId.Should().Be(2);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesReturnDetail
        {
            LotId = null,
            PackTypeId = null,
            SalesReturnHeader = null,
            InvoiceHeader = null,
            BagStatus = null
        };

        entity.LotId.Should().BeNull();
        entity.PackTypeId.Should().BeNull();
        entity.SalesReturnHeader.Should().BeNull();
        entity.InvoiceHeader.Should().BeNull();
        entity.BagStatus.Should().BeNull();
    }
}
