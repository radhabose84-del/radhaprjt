using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.Reports.SalesProjection.Dto;
using SalesManagement.Application.Reports.SalesProjection.Queries.GetSalesProjection;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class SalesProjectionControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private SalesProjectionController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetSalesProjection_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesProjectionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<SalesProjectionDto>
            {
                IsSuccess = true,
                Data = new SalesProjectionDto { Periods = new List<SalesProjectionPeriodDto>() }
            });

        var result = await CreateSut().GetSalesProjectionAsync(ProjectionPeriodType.Monthly);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSalesProjection_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesProjectionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<SalesProjectionDto> { IsSuccess = true, Data = new SalesProjectionDto() });

        await CreateSut().GetSalesProjectionAsync(ProjectionPeriodType.Monthly);

        _mockMediator.Verify(m => m.Send(It.IsAny<GetSalesProjectionQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
