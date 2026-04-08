using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.DeleteAttachment;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.RequestRework;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.SubmitFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UpdateFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UploadAttachment;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetAllFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbackByAssignment;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbackById;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbacksByComplaint;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetMyPendingFeedbacks;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class ComplaintDepartmentFeedbackControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private ComplaintDepartmentFeedbackController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllComplaintDepartmentFeedbackQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<FeedbackListDto>>
            {
                IsSuccess = true,
                Data = new List<FeedbackListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetComplaintDepartmentFeedbackByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<ComplaintDepartmentFeedbackDto>
            {
                IsSuccess = true,
                Data = new ComplaintDepartmentFeedbackDto { Id = 1 }
            });

        var result = await CreateSut().GetByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByAssignmentId_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetFeedbackByAssignmentIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<ComplaintDepartmentFeedbackDto>
            {
                IsSuccess = true,
                Data = new ComplaintDepartmentFeedbackDto { Id = 1 }
            });

        var result = await CreateSut().GetByAssignmentIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByComplaintId_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetFeedbacksByComplaintIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<FeedbackListDto>>
            {
                IsSuccess = true,
                Data = new List<FeedbackListDto>(),
                TotalCount = 0
            });

        var result = await CreateSut().GetByComplaintIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMyPending_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetMyPendingFeedbacksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<MyPendingFeedbackDto>>
            {
                IsSuccess = true,
                Data = new List<MyPendingFeedbackDto>(),
                TotalCount = 0
            });

        var result = await CreateSut().GetMyPendingAsync();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SubmitFeedback_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<SubmitComplaintDepartmentFeedbackCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Feedback submitted successfully.",
                Data = 1
            });

        var result = await CreateSut().SubmitFeedback(new SubmitComplaintDepartmentFeedbackCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateFeedback_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateComplaintDepartmentFeedbackCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Feedback updated successfully.",
                Data = 1
            });

        var result = await CreateSut().UpdateFeedback(new UpdateComplaintDepartmentFeedbackCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UploadAttachment_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UploadFeedbackAttachmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FeedbackAttachmentUploadDto());

        var result = await CreateSut().UploadAttachment(new UploadFeedbackAttachmentCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteAttachment_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteFeedbackAttachmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteAttachment(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RequestRework_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<RequestReworkCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Rework requested successfully.",
                Data = 1
            });

        var result = await CreateSut().RequestRework(new RequestReworkCommand());
        result.Should().BeOfType<OkObjectResult>();
    }
}
