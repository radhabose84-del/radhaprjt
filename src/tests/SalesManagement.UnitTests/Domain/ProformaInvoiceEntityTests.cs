using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class ProformaInvoiceEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new ProformaInvoice();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new ProformaInvoice();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(ProformaInvoice)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new ProformaInvoice
        {
            Id = 1,
            ProformaNumber = "PF-001",
            ProformaDate = new DateOnly(2026, 4, 16),
            SalesOrderId = 10,
            PartyId = 20,
            ProformaAmount = 50000.00m,
            SOBalance = 25000.00m,
            PaymentReceivedAmount = 25000.00m,
            StatusId = 3,
            Remarks = "Test remarks"
        };

        entity.Id.Should().Be(1);
        entity.ProformaNumber.Should().Be("PF-001");
        entity.ProformaDate.Should().Be(new DateOnly(2026, 4, 16));
        entity.SalesOrderId.Should().Be(10);
        entity.PartyId.Should().Be(20);
        entity.ProformaAmount.Should().Be(50000.00m);
        entity.SOBalance.Should().Be(25000.00m);
        entity.PaymentReceivedAmount.Should().Be(25000.00m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new ProformaInvoice
        {
            ProformaNumber = null,
            StatusId = null,
            Remarks = null,
            SalesOrderHeader = null,
            StatusMisc = null
        };

        entity.ProformaNumber.Should().BeNull();
        entity.StatusId.Should().BeNull();
        entity.Remarks.Should().BeNull();
        entity.SalesOrderHeader.Should().BeNull();
        entity.StatusMisc.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var salesOrder = new SalesOrderHeader();
        var statusMisc = new MiscMaster();

        var entity = new ProformaInvoice
        {
            SalesOrderHeader = salesOrder,
            StatusMisc = statusMisc
        };

        entity.SalesOrderHeader.Should().BeSameAs(salesOrder);
        entity.StatusMisc.Should().BeSameAs(statusMisc);
    }
}
