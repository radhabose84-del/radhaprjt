using Contracts.Common;
using InventoryManagement.Application.Issue.Command.CreateIssueEntry;
using InventoryManagement.Application.Issue.Queries.GetApprovedMrsById;
using InventoryManagement.Application.Issue.Queries.GetPendingIssue;
using InventoryManagement.Application.Issue.Queries.GetPendingIssueHeader;
using InventoryManagement.Presentation.Controllers.Issue;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class IssueControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

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
        public async Task CreateAsync_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateIssueEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(new CreateIssueEntryCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateIssueEntryCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetPendingIssuesDetails_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingIssueQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetPendingIssueDto> { new() });

            var result = await CreateSut().GetPendingIssuesDetails(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingIssueHeaderAsync_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingIssueHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetPendingIssueHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetPendingIssueHeaderDto> { new() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetPendingIssueHeaderAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMrsApprovedDetails_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetApprovedMrsByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetApprovedMrsByIdDto>());

            var result = await CreateSut().GetMrsApprovedDetails(null);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
