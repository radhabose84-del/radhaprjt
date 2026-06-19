using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class SalesQuotationAmendmentHeaderEntityTests
{
    [Fact]
    public void Header_DefaultIsActive_ShouldBeActive()
    {
        new SalesQuotationAmendmentHeader().IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void Header_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        new SalesQuotationAmendmentHeader().IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void Header_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(SalesQuotationAmendmentHeader)).Should().BeTrue();
    }

    [Fact]
    public void Header_Properties_ShouldBeAssignable()
    {
        var entity = new SalesQuotationAmendmentHeader
        {
            Id = 1,
            SalesQuotationHeaderId = 2,
            UnitId = 3,
            AmendmentNo = "Q1/AMD/1",
            RevisionNumber = 1,
            AmendmentDate = new DateOnly(2026, 1, 15),
            Reason = "revision",
            StatusId = 4,
            GrandTotal = 1000m
        };

        entity.SalesQuotationHeaderId.Should().Be(2);
        entity.UnitId.Should().Be(3);
        entity.AmendmentNo.Should().Be("Q1/AMD/1");
        entity.RevisionNumber.Should().Be(1);
        entity.StatusId.Should().Be(4);
        entity.GrandTotal.Should().Be(1000m);
    }

    [Fact]
    public void Header_NavigationProperty_ShouldBeAssignable()
    {
        var entity = new SalesQuotationAmendmentHeader
        {
            SalesQuotationAmendmentDetails = new List<SalesQuotationAmendmentDetail>()
        };
        entity.SalesQuotationAmendmentDetails.Should().NotBeNull();
    }

    [Fact]
    public void Detail_Properties_ShouldBeAssignable()
    {
        var detail = new SalesQuotationAmendmentDetail
        {
            Id = 1,
            SalesQuotationAmendmentHeaderId = 2,
            ChangeType = "Modified",
            SalesQuotationDetailId = 10,
            OldItemId = 5,
            OldQuantity = 10m,
            NewItemId = 6,
            NewQuantity = 8m,
            NetRate = 90m,
            TotalAmount = 720m,
            TaxAmount = 36m
        };

        detail.ChangeType.Should().Be("Modified");
        detail.SalesQuotationDetailId.Should().Be(10);
        detail.OldItemId.Should().Be(5);
        detail.NewItemId.Should().Be(6);
        detail.NetRate.Should().Be(90m);
    }
}
