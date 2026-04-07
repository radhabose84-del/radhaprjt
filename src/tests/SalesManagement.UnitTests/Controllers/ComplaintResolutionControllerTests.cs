using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.ComplaintResolution.Commands.SubmitResolution;
using SalesManagement.Application.ComplaintResolution.Commands.UpdateResolution;
using SalesManagement.Application.ComplaintResolution.Dto;
using SalesManagement.Application.ComplaintResolution.Queries.GetAllResolution;
using SalesManagement.Application.ComplaintResolution.Queries.GetResolutionByComplaintId;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class ComplaintResolutionControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private ComplaintResolutionController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllResolutionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<ResolutionListDto>>
            {
                IsSuccess = true,
                Data = new List<ResolutionListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllResolutionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<ResolutionListDto>>
            {
                IsSuccess = true,
                Data = new List<ResolutionListDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllResolutionQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByComplaintId_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetResolutionByComplaintIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComplaintResolutionDto { Id = 1 });

        var result = await CreateSut().GetByComplaintIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByComplaintId_WhenNull_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetResolutionByComplaintIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ComplaintResolutionDto?)null);

        var result = await CreateSut().GetByComplaintIdAsync(999);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SubmitResolution_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<SubmitResolutionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Resolution submitted successfully.",
                Data = 1
            });

        var result = await CreateSut().SubmitResolution(new SubmitResolutionCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateResolution_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateResolutionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Resolution updated successfully.",
                Data = 1
            });

        var result = await CreateSut().UpdateResolution(new UpdateResolutionCommand());
        result.Should().BeOfType<OkObjectResult>();
    }
}
