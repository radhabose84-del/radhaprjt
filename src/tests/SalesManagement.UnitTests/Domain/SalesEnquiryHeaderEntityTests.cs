using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class SalesEnquiryHeaderEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new SalesEnquiryHeader();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new SalesEnquiryHeader();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(SalesEnquiryHeader)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new SalesEnquiryHeader
        {
            Id = 1,
            PartyId = 10,
            EnquiryDate = DateTimeOffset.UtcNow,
            ContactPerson = "John",
            Remarks = "Test remarks",
            PaymentTermId = 5,
            SalesLeadId = 3
        };

        entity.Id.Should().Be(1);
        entity.PartyId.Should().Be(10);
        entity.ContactPerson.Should().Be("John");
        entity.Remarks.Should().Be("Test remarks");
        entity.PaymentTermId.Should().Be(5);
        entity.SalesLeadId.Should().Be(3);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesEnquiryHeader
        {
            ContactPerson = null,
            ExpectedDeliveryDate = null,
            PaymentTermId = null,
            SalesLeadId = null,
            Remarks = null
        };

        entity.ContactPerson.Should().BeNull();
        entity.ExpectedDeliveryDate.Should().BeNull();
        entity.PaymentTermId.Should().BeNull();
        entity.SalesLeadId.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var entity = new SalesEnquiryHeader
        {
            SalesEnquiryDetails = new List<SalesEnquiryDetail>
            {
                new() { ItemId = 1, Quantity = 10m }
            }
        };

        entity.SalesEnquiryDetails.Should().HaveCount(1);
    }
}
