using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class SalesContactEntityTests
{
    [Fact]
    public void SalesContact_DefaultIsActive_ShouldBeActive()
    {
        var entity = new SalesContact();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void SalesContact_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new SalesContact();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void SalesContact_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(SalesContact)).Should().BeTrue();
    }

    [Fact]
    public void SalesContact_Properties_ShouldBeAssignable()
    {
        var entity = new SalesContact
        {
            Id = 1,
            ContactName = "John Doe",
            MobileNumber = "9876543210",
            ContactTypeId = 5,
            PartyId = 10,
            Email = "john@test.com",
            Remarks = "Test remark"
        };

        entity.Id.Should().Be(1);
        entity.ContactName.Should().Be("John Doe");
        entity.MobileNumber.Should().Be("9876543210");
        entity.ContactTypeId.Should().Be(5);
        entity.PartyId.Should().Be(10);
        entity.Email.Should().Be("john@test.com");
        entity.Remarks.Should().Be("Test remark");
    }

    [Fact]
    public void SalesContact_NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesContact
        {
            ContactName = null,
            MobileNumber = null,
            PartyId = null,
            Email = null,
            Remarks = null
        };

        entity.ContactName.Should().BeNull();
        entity.MobileNumber.Should().BeNull();
        entity.PartyId.Should().BeNull();
        entity.Email.Should().BeNull();
        entity.Remarks.Should().BeNull();
    }

    [Fact]
    public void SalesContact_NavigationProperty_ShouldBeAssignable()
    {
        var entity = new SalesContact
        {
            ContactType = new MiscMaster()
        };

        entity.ContactType.Should().NotBeNull();
    }
}
