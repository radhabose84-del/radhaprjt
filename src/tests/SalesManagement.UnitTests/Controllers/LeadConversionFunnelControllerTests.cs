using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.LeadConversionFunnel.Dto;
using SalesManagement.Application.LeadConversionFunnel.Queries.GetLeadConversionFunnel;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class LeadConversionFunnelControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private LeadConversionFunnelController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetLeadConversionFunnel_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetLeadConversionFunnelQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<LeadConversionFunnelDto>
            {
                IsSuccess = true,
                Message = "Lead conversion funnel retrieved successfully.",
                Data = new LeadConversionFunnelDto()
            });

        var result = await CreateSut().GetLeadConversionFunnelAsync();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetLeadConversionFunnel_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetLeadConversionFunnelQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<LeadConversionFunnelDto>
            {
                IsSuccess = true,
                Data = new LeadConversionFunnelDto()
            });

        await CreateSut().GetLeadConversionFunnelAsync();

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetLeadConversionFunnelQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetLeadConversionFunnel_PropagatesData_FromResponse()
    {
        var payload = new LeadConversionFunnelDto
        {
            Officers = new List<OfficerFunnelDto>
            {
                new() { MarketingOfficerId = 1, EmployeeNo = "E001", EmployeeName = "Alice" }
            }
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetLeadConversionFunnelQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<LeadConversionFunnelDto>
            {
                IsSuccess = true,
                Data = payload
            });

        var result = await CreateSut().GetLeadConversionFunnelAsync() as OkObjectResult;

        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
    }
}
