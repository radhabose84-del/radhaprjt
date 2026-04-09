using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.AgentPortal.Dto;
using SalesManagement.Application.AgentPortal.Queries.GetAgentDashboard;
using SalesManagement.Application.AgentPortal.Queries.GetAgentPagedData;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class AgentPortalControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private AgentPortalController CreateSut() => new(_mockMediator.Object);

    // ── GetDashboard ─────────────────────────────────────────────

    [Fact]
    public async Task GetDashboard_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentDashboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<AgentDashboardDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = new AgentDashboardDto()
            });

        var result = await CreateSut().GetDashboardAsync();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDashboard_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentDashboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<AgentDashboardDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = new AgentDashboardDto()
            });

        await CreateSut().GetDashboardAsync();

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAgentDashboardQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetMyCustomers ───────────────────────────────────────────

    [Fact]
    public async Task GetMyCustomers_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentMyCustomersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentCustomerDto>>
            {
                IsSuccess = true,
                Data = new List<AgentCustomerDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetMyCustomersAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMyCustomers_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentMyCustomersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentCustomerDto>>
            {
                IsSuccess = true,
                Data = new List<AgentCustomerDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        await CreateSut().GetMyCustomersAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAgentMyCustomersQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetEnquiries ─────────────────────────────────────────────

    [Fact]
    public async Task GetEnquiries_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentEnquiriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentEnquiryListDto>>
            {
                IsSuccess = true,
                Data = new List<AgentEnquiryListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetEnquiriesAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetEnquiries_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentEnquiriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentEnquiryListDto>>
            {
                IsSuccess = true,
                Data = new List<AgentEnquiryListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        await CreateSut().GetEnquiriesAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAgentEnquiriesQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetSalesOrders ───────────────────────────────────────────

    [Fact]
    public async Task GetSalesOrders_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentSalesOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentSalesOrderListDto>>
            {
                IsSuccess = true,
                Data = new List<AgentSalesOrderListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetSalesOrdersAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSalesOrders_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentSalesOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentSalesOrderListDto>>
            {
                IsSuccess = true,
                Data = new List<AgentSalesOrderListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        await CreateSut().GetSalesOrdersAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAgentSalesOrdersQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetComplaints ────────────────────────────────────────────

    [Fact]
    public async Task GetComplaints_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentComplaintsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentComplaintListDto>>
            {
                IsSuccess = true,
                Data = new List<AgentComplaintListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetComplaintsAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetComplaints_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentComplaintsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentComplaintListDto>>
            {
                IsSuccess = true,
                Data = new List<AgentComplaintListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        await CreateSut().GetComplaintsAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAgentComplaintsQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetInvoices ──────────────────────────────────────────────

    [Fact]
    public async Task GetInvoices_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentInvoicesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentInvoiceListDto>>
            {
                IsSuccess = true,
                Data = new List<AgentInvoiceListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetInvoicesAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetInvoices_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentInvoicesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentInvoiceListDto>>
            {
                IsSuccess = true,
                Data = new List<AgentInvoiceListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        await CreateSut().GetInvoicesAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAgentInvoicesQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetDispatches ────────────────────────────────────────────

    [Fact]
    public async Task GetDispatches_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentDispatchesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentDispatchListDto>>
            {
                IsSuccess = true,
                Data = new List<AgentDispatchListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetDispatchesAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDispatches_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentDispatchesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentDispatchListDto>>
            {
                IsSuccess = true,
                Data = new List<AgentDispatchListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        await CreateSut().GetDispatchesAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAgentDispatchesQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetCommissions ───────────────────────────────────────────

    [Fact]
    public async Task GetCommissions_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentCommissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentCommissionDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = new List<AgentCommissionDto>()
            });

        var result = await CreateSut().GetCommissionsAsync();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetCommissions_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAgentCommissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AgentCommissionDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = new List<AgentCommissionDto>()
            });

        await CreateSut().GetCommissionsAsync();

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAgentCommissionsQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
