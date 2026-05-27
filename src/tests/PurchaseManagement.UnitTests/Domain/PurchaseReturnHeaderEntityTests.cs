using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseReturn;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain;

public class PurchaseReturnHeaderEntityTests
{
    [Fact]
    public void PurchaseReturnHeader_DefaultIsActive_ShouldBeActive()
    {
        var entity = new PurchaseReturnHeader();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void PurchaseReturnHeader_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new PurchaseReturnHeader();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void PurchaseReturnHeader_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseReturnHeader)).Should().BeTrue();
    }

    [Fact]
    public void PurchaseReturnHeader_Properties_ShouldBeAssignable()
    {
        var entity = new PurchaseReturnHeader
        {
            Id = 1,
            RtvNumber = "RTV/2026/0001",
            RtvDate = new DateOnly(2026, 5, 27),
            UnitId = 37,
            VendorId = 42,
            PoId = 100,
            GrnHeaderId = 200,
            ReturnTypeId = 1,
            ReturnReasonId = 2,
            ReturnActionId = 3,
            IsReplacementRequired = true,
            IsDebitNoteRequired = true,
            IsQcVerified = true,
            Remarks = "test",
            StatusId = 1,
            ApprovalRequestId = 99,
            ReplacementStatusId = 5,
            ReplacementClosedDate = new DateTimeOffset(2026, 5, 27, 0, 0, 0, TimeSpan.Zero)
        };
        entity.RtvNumber.Should().Be("RTV/2026/0001");
        entity.UnitId.Should().Be(37);
        entity.VendorId.Should().Be(42);
        entity.ReturnTypeId.Should().Be(1);
        entity.IsReplacementRequired.Should().BeTrue();
        entity.ApprovalRequestId.Should().Be(99);
    }

    [Fact]
    public void PurchaseReturnHeader_NullableProperties_ShouldAcceptNull()
    {
        var entity = new PurchaseReturnHeader
        {
            Remarks = null,
            ApprovalRequestId = null,
            ReplacementStatusId = null,
            ReplacementClosedDate = null
        };
        entity.Remarks.Should().BeNull();
        entity.ApprovalRequestId.Should().BeNull();
        entity.ReplacementStatusId.Should().BeNull();
        entity.ReplacementClosedDate.Should().BeNull();
    }

    [Fact]
    public void PurchaseReturnHeader_Details_DefaultEmpty()
    {
        var entity = new PurchaseReturnHeader();
        entity.Details.Should().NotBeNull().And.BeEmpty();
    }
}
