using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class SalesAgreementHeaderEntityTests
{
    [Fact]
    public void SalesAgreementHeader_DefaultIsActive_ShouldBeActive()
    {
        new SalesAgreementHeader().IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void SalesAgreementHeader_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        new SalesAgreementHeader().IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void SalesAgreementHeader_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(SalesAgreementHeader)).Should().BeTrue();
    }

    [Fact]
    public void SalesAgreementHeader_Properties_ShouldBeAssignable()
    {
        var entity = new SalesAgreementHeader
        {
            Id = 1,
            AgreementNo = "SA001",
            StatusId = 2,
            ValidFrom = new DateOnly(2026, 1, 1),
            ValidTo = new DateOnly(2026, 12, 31),
            CustomerId = 3,
            SalesGroupId = 4,
            PaymentTermsId = 5,
            Remarks = "remarks",
            CustomerPoRefno = "PO-1",
            AgentPOAttachment = "file.pdf",
            UnitId = 6
        };

        entity.AgreementNo.Should().Be("SA001");
        entity.StatusId.Should().Be(2);
        entity.ValidFrom.Should().Be(new DateOnly(2026, 1, 1));
        entity.ValidTo.Should().Be(new DateOnly(2026, 12, 31));
        entity.CustomerId.Should().Be(3);
        entity.SalesGroupId.Should().Be(4);
        entity.PaymentTermsId.Should().Be(5);
        entity.UnitId.Should().Be(6);
    }

    [Fact]
    public void SalesAgreementHeader_NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesAgreementHeader { AgreementNo = null, Remarks = null, UnitId = null };
        entity.AgreementNo.Should().BeNull();
        entity.Remarks.Should().BeNull();
        entity.UnitId.Should().BeNull();
    }

    [Fact]
    public void SalesAgreementHeader_NavigationProperties_ShouldBeAssignable()
    {
        var entity = new SalesAgreementHeader { SalesAgreementDetails = new List<SalesAgreementDetail>() };
        entity.SalesAgreementDetails.Should().NotBeNull();
    }

    [Fact]
    public void SalesAgreementDetail_Properties_ShouldBeAssignable()
    {
        var detail = new SalesAgreementDetail
        {
            Id = 1,
            SalesAgreementHeaderId = 2,
            ItemId = 3,
            VariantId = 4,
            UomId = 5,
            AgreedRate = 10m,
            TotalQty = 100m,
            ReleasedQty = 20m
        };

        detail.SalesAgreementHeaderId.Should().Be(2);
        detail.ItemId.Should().Be(3);
        detail.AgreedRate.Should().Be(10m);
        detail.TotalQty.Should().Be(100m);
        detail.ReleasedQty.Should().Be(20m);
    }
}
