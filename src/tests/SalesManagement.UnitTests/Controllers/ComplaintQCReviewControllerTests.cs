using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.ComplaintQCReview.Commands.SubmitQCReview;
using SalesManagement.Application.ComplaintQCReview.Commands.UpdateQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;
using SalesManagement.Application.ComplaintQCReview.Queries.GetAllQCReview;
using SalesManagement.Application.ComplaintQCReview.Queries.GetQCReviewByComplaintId;
using SalesManagement.Application.ComplaintQCReview.Queries.GetQCReviewById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class ComplaintQCReviewControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private ComplaintQCReviewController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllQCReviewQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<QCReviewListDto>>
            {
                IsSuccess = true,
                Data = new List<QCReviewListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllQCReviewAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllQCReviewQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<QCReviewListDto>>
            {
                IsSuccess = true,
                Data = new List<QCReviewListDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllQCReviewAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllQCReviewQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetQCReviewByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComplaintQCReviewDto { Id = 1 });

        var result = await CreateSut().GetQCReviewByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByComplaintId_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetQCReviewByComplaintIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComplaintQCReviewDto { Id = 1 });

        var result = await CreateSut().GetQCReviewByComplaintIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SubmitQCReview_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<SubmitQCReviewCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "QC Review submitted successfully.",
                Data = 1
            });

        var result = await CreateSut().SubmitQCReview(new SubmitQCReviewCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateQCReview_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateQCReviewCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "QC Review updated successfully.",
                Data = 1
            });

        var result = await CreateSut().UpdateQCReview(new UpdateQCReviewCommand());
        result.Should().BeOfType<OkObjectResult>();
    }
}
