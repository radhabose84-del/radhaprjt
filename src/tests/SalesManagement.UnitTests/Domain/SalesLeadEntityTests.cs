using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class SalesLeadEntityTests
{
    [Fact]
    public void SalesLead_DefaultIsActive_ShouldBeActive()
    {
        var entity = new SalesLead();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void SalesLead_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new SalesLead();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void SalesLead_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(SalesLead)).Should().BeTrue();
    }

    [Fact]
    public void SalesLead_Properties_ShouldBeAssignable()
    {
        var entity = new SalesLead
        {
            Id = 1,
            PartyId = 10,
            ProspectCompanyName = "Acme Corp",
            CityId = 5,
            ContactName = "Jane Doe",
            MobileNumber = "9876543210",
            EmailId = "jane@acme.com",
            ContactId = 3,
            ItemId = 7,
            RequirementQty = 100.5m,
            ExpectedDate = new DateOnly(2026, 6, 15),
            Remarks = "Hot lead",
            LeadSourceId = 2,
            MarketingOfficerId = 4,
            InteractionDate = DateTimeOffset.UtcNow
        };

        entity.Id.Should().Be(1);
        entity.PartyId.Should().Be(10);
        entity.ProspectCompanyName.Should().Be("Acme Corp");
        entity.CityId.Should().Be(5);
        entity.ContactName.Should().Be("Jane Doe");
        entity.MobileNumber.Should().Be("9876543210");
        entity.EmailId.Should().Be("jane@acme.com");
        entity.ContactId.Should().Be(3);
        entity.ItemId.Should().Be(7);
        entity.RequirementQty.Should().Be(100.5m);
        entity.ExpectedDate.Should().Be(new DateOnly(2026, 6, 15));
        entity.Remarks.Should().Be("Hot lead");
        entity.LeadSourceId.Should().Be(2);
        entity.MarketingOfficerId.Should().Be(4);
    }

    [Fact]
    public void SalesLead_NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesLead
        {
            PartyId = null,
            ProspectCompanyName = null,
            CityId = null,
            ContactName = null,
            MobileNumber = null,
            EmailId = null,
            ContactId = null,
            ItemId = null,
            RequirementQty = null,
            ExpectedDate = null,
            Remarks = null,
            LeadSourceId = null
        };

        entity.PartyId.Should().BeNull();
        entity.CityId.Should().BeNull();
        entity.ContactId.Should().BeNull();
        entity.ItemId.Should().BeNull();
        entity.RequirementQty.Should().BeNull();
        entity.ExpectedDate.Should().BeNull();
        entity.LeadSourceId.Should().BeNull();
    }

    [Fact]
    public void SalesLead_NavigationProperties_ShouldBeAssignable()
    {
        var entity = new SalesLead
        {
            Contact = new SalesContact(),
            LeadSource = new MiscMaster(),
            MarketingOfficer = new MarketingOfficer(),
            SalesEnquiryHeaders = new List<SalesEnquiryHeader>()
        };

        entity.Contact.Should().NotBeNull();
        entity.LeadSource.Should().NotBeNull();
        entity.MarketingOfficer.Should().NotBeNull();
        entity.SalesEnquiryHeaders.Should().NotBeNull();
    }
}
