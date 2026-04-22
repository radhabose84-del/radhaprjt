using Contracts.Common;
using Contracts.Interfaces;
using SalesManagement.Application.AgentPortal.Dto;
using SalesManagement.Application.AgentPortal.Queries.GetAgentPagedData;
using SalesManagement.Application.Common.Interfaces.IAgentPortal;

namespace SalesManagement.UnitTests.Application.AgentPortal.Queries;

public sealed class AgentPagedDataQueryHandlerTests
{
    private readonly Mock<IAgentPortalQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Strict);

    // ── GetAgentMyCustomers ──────────────────────────────────────

    [Fact]
    public async Task MyCustomers_WhenAgentIdentified_ReturnsSuccess()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns(10);
        _mockQueryRepo
            .Setup(r => r.GetMyCustomersAsync(10, 1, 10, null))
            .ReturnsAsync((new List<AgentCustomerDto>(), 0));

        var handler = new GetAgentMyCustomersQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentMyCustomersQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task MyCustomers_WhenPartyIdNull_ReturnsFailure()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns((int?)null);

        var handler = new GetAgentMyCustomersQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentMyCustomersQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Agent not identified.");
    }

    [Fact]
    public async Task MyCustomers_ReturnsPaginationMetadata()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns(10);
        _mockQueryRepo
            .Setup(r => r.GetMyCustomersAsync(10, 2, 5, "search"))
            .ReturnsAsync((new List<AgentCustomerDto> { new() }, 11));

        var handler = new GetAgentMyCustomersQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentMyCustomersQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
            CancellationToken.None);

        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(11);
    }

    // ── GetAgentEnquiries ────────────────────────────────────────

    [Fact]
    public async Task Enquiries_WhenAgentIdentified_ReturnsSuccess()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns(10);
        _mockQueryRepo.Setup(r => r.GetAgentCustomerIdsAsync(10)).ReturnsAsync(new List<int> { 1 });
        _mockQueryRepo
            .Setup(r => r.GetEnquiriesAsync(It.IsAny<List<int>>(), 1, 10, null))
            .ReturnsAsync((new List<AgentEnquiryListDto>(), 0));

        var handler = new GetAgentEnquiriesQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentEnquiriesQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Enquiries_WhenPartyIdNull_ReturnsFailure()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns((int?)null);

        var handler = new GetAgentEnquiriesQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentEnquiriesQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    // ── GetAgentSalesOrders ──────────────────────────────────────

    [Fact]
    public async Task SalesOrders_WhenAgentIdentified_ReturnsSuccess()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns(10);
        _mockQueryRepo.Setup(r => r.GetAgentCustomerIdsAsync(10)).ReturnsAsync(new List<int> { 1 });
        _mockQueryRepo
            .Setup(r => r.GetSalesOrdersAsync(It.IsAny<int>(), 1, 10, null))
            .ReturnsAsync((new List<AgentSalesOrderListDto>(), 0));

        var handler = new GetAgentSalesOrdersQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentSalesOrdersQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SalesOrders_WhenPartyIdNull_ReturnsFailure()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns((int?)null);

        var handler = new GetAgentSalesOrdersQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentSalesOrdersQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    // ── GetAgentComplaints ───────────────────────────────────────

    [Fact]
    public async Task Complaints_WhenAgentIdentified_ReturnsSuccess()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns(10);
        _mockQueryRepo.Setup(r => r.GetAgentCustomerIdsAsync(10)).ReturnsAsync(new List<int> { 1 });
        _mockQueryRepo
            .Setup(r => r.GetComplaintsAsync(It.IsAny<List<int>>(), 1, 10, null))
            .ReturnsAsync((new List<AgentComplaintListDto>(), 0));

        var handler = new GetAgentComplaintsQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentComplaintsQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Complaints_WhenPartyIdNull_ReturnsFailure()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns((int?)null);

        var handler = new GetAgentComplaintsQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentComplaintsQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    // ── GetAgentInvoices ─────────────────────────────────────────

    [Fact]
    public async Task Invoices_WhenAgentIdentified_ReturnsSuccess()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns(10);
        _mockQueryRepo.Setup(r => r.GetAgentCustomerIdsAsync(10)).ReturnsAsync(new List<int> { 1 });
        _mockQueryRepo
            .Setup(r => r.GetInvoicesAsync(It.IsAny<List<int>>(), 1, 10, null))
            .ReturnsAsync((new List<AgentInvoiceListDto>(), 0));

        var handler = new GetAgentInvoicesQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentInvoicesQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Invoices_WhenPartyIdNull_ReturnsFailure()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns((int?)null);

        var handler = new GetAgentInvoicesQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentInvoicesQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    // ── GetAgentDispatches ───────────────────────────────────────

    [Fact]
    public async Task Dispatches_WhenAgentIdentified_ReturnsSuccess()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns(10);
        _mockQueryRepo.Setup(r => r.GetAgentCustomerIdsAsync(10)).ReturnsAsync(new List<int> { 1 });
        _mockQueryRepo
            .Setup(r => r.GetDispatchesAsync(It.IsAny<List<int>>(), 1, 10, null))
            .ReturnsAsync((new List<AgentDispatchListDto>(), 0));

        var handler = new GetAgentDispatchesQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentDispatchesQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Dispatches_WhenPartyIdNull_ReturnsFailure()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns((int?)null);

        var handler = new GetAgentDispatchesQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(
            new GetAgentDispatchesQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    // ── GetAgentCommissions ──────────────────────────────────────

    [Fact]
    public async Task Commissions_WhenAgentIdentified_ReturnsSuccess()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns(10);
        _mockQueryRepo
            .Setup(r => r.GetCommissionsAsync(10))
            .ReturnsAsync(new List<AgentCommissionDto>());

        var handler = new GetAgentCommissionsQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(new GetAgentCommissionsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Commissions_WhenPartyIdNull_ReturnsFailure()
    {
        _mockIpService.Setup(s => s.GetPartyId()).Returns((int?)null);

        var handler = new GetAgentCommissionsQueryHandler(_mockQueryRepo.Object, _mockIpService.Object);
        var result = await handler.Handle(new GetAgentCommissionsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Agent not identified.");
    }
}
