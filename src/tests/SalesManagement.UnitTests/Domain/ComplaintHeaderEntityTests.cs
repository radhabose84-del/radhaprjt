using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class ComplaintHeaderEntityTests
{
    [Fact]
    public void ComplaintHeader_DefaultIsActive_ShouldBeActive()
    {
        var entity = new ComplaintHeader();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void ComplaintHeader_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new ComplaintHeader();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ComplaintHeader_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(ComplaintHeader)).Should().BeTrue();
    }

    [Fact]
    public void ComplaintHeader_Properties_ShouldBeAssignable()
    {
        var entity = new ComplaintHeader
        {
            Id = 1,
            ComplaintNumber = "CMP001",
            ComplaintDate = new DateOnly(2026, 1, 1),
            CustomerId = 10,
            CustomerAddress = "123 Main St",
            CustomerPIN = "400001",
            CustomerMobile = "9876543210",
            CustomerEmail = "test@example.com",
            CustomerPAN = "ABCDE1234F",
            CustomerGSTNo = "22AAAAA1234A1Z5",
            CreditLimit = 100000m,
            TotalOS = 50000m,
            Outstanding = 25000m,
            BalanceCredit = 75000m,
            Delay = "30 days",
            Ledger = "Active",
            StatusId = 5,
            Remarks = "Test remarks"
        };

        entity.Id.Should().Be(1);
        entity.ComplaintNumber.Should().Be("CMP001");
        entity.ComplaintDate.Should().Be(new DateOnly(2026, 1, 1));
        entity.CustomerId.Should().Be(10);
        entity.CustomerAddress.Should().Be("123 Main St");
        entity.StatusId.Should().Be(5);
        entity.Remarks.Should().Be("Test remarks");
        entity.CreditLimit.Should().Be(100000m);
    }

    [Fact]
    public void ComplaintHeader_NullableProperties_ShouldAcceptNull()
    {
        var entity = new ComplaintHeader
        {
            ComplaintNumber = null,
            CustomerAddress = null,
            StatusId = null,
            Remarks = null,
            Status = null,
            ComplaintDetails = null
        };

        entity.ComplaintNumber.Should().BeNull();
        entity.StatusId.Should().BeNull();
        entity.ComplaintDetails.Should().BeNull();
    }

    [Fact]
    public void ComplaintHeader_NavigationProperties_ShouldBeAssignable()
    {
        var entity = new ComplaintHeader
        {
            Status = new MiscMaster { Id = 5 },
            ComplaintDetails = new List<ComplaintDetail>
            {
                new ComplaintDetail { Id = 1 }
            }
        };

        entity.Status.Should().NotBeNull();
        entity.ComplaintDetails.Should().HaveCount(1);
    }
}
