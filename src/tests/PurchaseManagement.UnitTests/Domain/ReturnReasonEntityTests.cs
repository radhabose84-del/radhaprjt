using PurchaseManagement.Domain.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;
using DomainReturnReason = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnReason;

namespace PurchaseManagement.UnitTests.Domain;

public class ReturnReasonEntityTests
{
    [Fact]
    public void ReturnReason_DefaultIsActive_ShouldBeActive()
    {
        var entity = new DomainReturnReason();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void ReturnReason_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new DomainReturnReason();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ReturnReason_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(DomainReturnReason)).Should().BeTrue();
    }

    [Fact]
    public void ReturnReason_Properties_ShouldBeAssignable()
    {
        var entity = new DomainReturnReason
        {
            Id = 1,
            Code = "MoistureFailure",
            Description = "Moisture Failure",
            ReturnTypeId = 5,
            IsReplacementOverride = true,
            IsDebitNoteOverride = false,
            IsQcMandatoryOverride = true
        };
        entity.Code.Should().Be("MoistureFailure");
        entity.ReturnTypeId.Should().Be(5);
        entity.IsReplacementOverride.Should().BeTrue();
        entity.IsDebitNoteOverride.Should().BeFalse();
        entity.IsQcMandatoryOverride.Should().BeTrue();
    }

    [Fact]
    public void ReturnReason_NullableOverrides_ShouldAcceptNull()
    {
        var entity = new DomainReturnReason
        {
            IsReplacementOverride = null,
            IsDebitNoteOverride = null,
            IsQcMandatoryOverride = null
        };
        entity.IsReplacementOverride.Should().BeNull();
        entity.IsDebitNoteOverride.Should().BeNull();
        entity.IsQcMandatoryOverride.Should().BeNull();
    }

    [Fact]
    public void ReturnReason_ChildCollections_DefaultEmpty()
    {
        var entity = new DomainReturnReason();
        entity.PurchaseReturns.Should().NotBeNull().And.BeEmpty();
        entity.PurchaseReturnDetailReasons.Should().NotBeNull().And.BeEmpty();
    }
}
