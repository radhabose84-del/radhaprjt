using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Issue.Command.CreateIssueEntry;
using PurchaseManagement.Application.Issue.Queries.GetPendingIssue;
using PurchaseManagement.Application.Issue.Queries.GetPendingIssueHeader;
using PurchaseManagement.Application.Issue.Queries.GetApprovedMrsById;
using PurchaseManagement.Presentation.Controllers.Issue;
using Contracts.Common;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class IssueControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private IssueController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateIssueEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateIssueEntryCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingIssuesDetails_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingIssueQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetPendingIssueDto> { new GetPendingIssueDto() }));

            var result = await CreateSut().GetPendingIssuesDetails(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingIssuesDetails_WhenEmpty_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingIssueQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetPendingIssueDto>()));

            var result = await CreateSut().GetPendingIssuesDetails(999, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetPendingIssueHeader_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingIssueHeaderQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ApiResponseDTO<List<GetPendingIssueHeaderDto>>
                {
                    Data = new List<GetPendingIssueHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                }));

            var result = await CreateSut().GetPendingIssueHeaderAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMrsApprovedDetails_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetApprovedMrsByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetApprovedMrsByIdDto>()));

            var result = await CreateSut().GetMrsApprovedDetails("MRS001");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateIssueEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(new CreateIssueEntryCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateIssueEntryCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
