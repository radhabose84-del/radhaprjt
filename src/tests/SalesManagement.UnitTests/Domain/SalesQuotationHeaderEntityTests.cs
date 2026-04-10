using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class SalesQuotationHeaderEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new SalesQuotationHeader();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new SalesQuotationHeader();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(SalesQuotationHeader)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new SalesQuotationHeader
        {
            Id = 1,
            CustomerId = 10,
            QuotationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentTermId = 5,
            DeliveryTermId = 3,
            FreightCharges = 100m,
            OtherCharges = 50m,
            GrandTotal = 5000m,
            Remarks = "Test"
        };

        entity.Id.Should().Be(1);
        entity.CustomerId.Should().Be(10);
        entity.PaymentTermId.Should().Be(5);
        entity.GrandTotal.Should().Be(5000m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesQuotationHeader
        {
            SalesEnquiryId = null,
            ContactPersonId = null,
            StatusId = null,
            Remarks = null
        };

        entity.SalesEnquiryId.Should().BeNull();
        entity.ContactPersonId.Should().BeNull();
        entity.StatusId.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var entity = new SalesQuotationHeader
        {
            SalesQuotationDetails = new List<SalesQuotationDetail>
            {
                new() { Id = 1 }
            }
        };

        entity.SalesQuotationDetails.Should().HaveCount(1);
    }
}
