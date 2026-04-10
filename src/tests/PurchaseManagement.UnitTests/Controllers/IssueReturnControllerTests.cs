using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn;
using PurchaseManagement.Application.IssueReturn.Command.UpdateIssueReturn;
using PurchaseManagement.Application.IssueReturn.Queries.GetIssueDetailsById;
using PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturn;
using PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturnById;
using PurchaseManagement.Presentation.Controllers.IssueReturn;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class IssueReturnControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private IssueReturnController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateIssueReturnEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateIssueReturnEntryCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetIssueDetailsById_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetIssueDetailsByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetIssueDetailsByIdDto> { new GetIssueDetailsByIdDto() }));

            var result = await CreateSut().GetIssueDetailsById(1, null, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetIssueDetailsById_WhenEmpty_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetIssueDetailsByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetIssueDetailsByIdDto>()));

            var result = await CreateSut().GetIssueDetailsById(999, null, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            var result = await CreateSut().UpdateAsync(new UpdateIssueReturnEntryCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingIssueReturn_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingIssueReturnQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ApiResponseDTO<List<PendingIssueReturnDto>>
                {
                    Data = new List<PendingIssueReturnDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                }));

            var result = await CreateSut().GetPendingIssueReturnAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingIssueReturnById_ReturnsOkResult()
        {
            var result = await CreateSut().GetPendingIssueReturnByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateIssueReturnEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(new CreateIssueReturnEntryCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateIssueReturnEntryCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
