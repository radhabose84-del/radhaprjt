using MediatR;
using Microsoft.AspNetCore.Mvc;
using BackgroundService.Presentation.Controllers.Workflow;
using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest;
using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveDocumentUpload;
using BackgroundService.Application.Workflow.ApprovalRequests.Queries.ApprovalDocumentDownload;
using BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovedHistory;
using BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestById;
using BackgroundService.Application.Workflow.Common;

namespace BackgroundService.UnitTests.Controllers.Workflow
{
    public sealed class ApprovalRequestControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ApprovalRequestController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task ApproveBatchAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ApproveApprovalRequestBatchCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApproveBatchResultDto
                {
                    ApprovedCount = 1,
                    RejectedCount = 0,
                    FailedCount = 0,
                    Errors = new List<string>()
                });

            var result = await CreateSut().ApproveBatchAsync(new ApproveApprovalRequestBatchCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ApproveBatchAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ApproveApprovalRequestBatchCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApproveBatchResultDto
                {
                    ApprovedCount = 1,
                    RejectedCount = 0,
                    FailedCount = 0,
                    Errors = new List<string>()
                });

            await CreateSut().ApproveBatchAsync(new ApproveApprovalRequestBatchCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<ApproveApprovalRequestBatchCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UploadFile_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUploadResult());

            var result = await CreateSut().UploadFile(new UploadFileCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DownloadFile_ReturnsFileResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DownloadFileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DownloadFileResult
                {
                    FileBytes = new byte[] { 1, 2, 3 },
                    ContentType = "application/pdf",
                    FileName = "test.pdf"
                });

            var result = await CreateSut().DownloadFile("path/to/file");

            result.Should().BeOfType<FileContentResult>();
        }

        [Fact]
        public async Task GetApprovedHistoryAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetApprovedHistoryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ApprovedHistoryDto>());

            var result = await CreateSut().GetApprovedHistoryAsync(new GetApprovedHistoryQuery());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByModule_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetApprovalRequestByModuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ApprovalRequestWithLinesDto>());

            var result = await CreateSut().GetByModule(1, 1);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByModule_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetApprovalRequestByModuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ApprovalRequestWithLinesDto>());

            await CreateSut().GetByModule(1, 1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetApprovalRequestByModuleQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
