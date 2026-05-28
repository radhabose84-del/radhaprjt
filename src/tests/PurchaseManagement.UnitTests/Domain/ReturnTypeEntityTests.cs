using PurchaseManagement.Domain.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;
using DomainReturnType = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnType;

namespace PurchaseManagement.UnitTests.Domain;

public class ReturnTypeEntityTests
{
    [Fact]
    public void ReturnType_DefaultIsActive_ShouldBeActive()
    {
        var entity = new DomainReturnType();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void ReturnType_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new DomainReturnType();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ReturnType_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(DomainReturnType)).Should().BeTrue();
    }

    [Fact]
    public void ReturnType_Properties_ShouldBeAssignable()
    {
        var entity = new DomainReturnType
        {
            Id = 1,
            Code = "Rejected",
            Description = "Rejected",
            InventoryImpactId = 10,
            FinanceImpactId = 20,
            IsReplacementApplicable = true,
            IsQcMandatory = true,
            ApprovalRoleCode = "QcHead"
        };
        entity.Code.Should().Be("Rejected");
        entity.InventoryImpactId.Should().Be(10);
        entity.FinanceImpactId.Should().Be(20);
        entity.IsReplacementApplicable.Should().BeTrue();
        entity.IsQcMandatory.Should().BeTrue();
        entity.ApprovalRoleCode.Should().Be("QcHead");
    }

    [Fact]
    public void ReturnType_NullableProperties_ShouldAcceptNull()
    {
        var entity = new DomainReturnType
        {
            InventoryImpactId = null,
            FinanceImpactId = null,
            ApprovalRoleCode = null
        };
        entity.InventoryImpactId.Should().BeNull();
        entity.FinanceImpactId.Should().BeNull();
        entity.ApprovalRoleCode.Should().BeNull();
    }

    [Fact]
    public void ReturnType_ChildCollections_DefaultEmpty()
    {
        var entity = new DomainReturnType();
        entity.Reasons.Should().NotBeNull().And.BeEmpty();
        entity.PurchaseReturns.Should().NotBeNull().And.BeEmpty();
    }
}
