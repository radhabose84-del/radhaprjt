using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Application.Issue.Queries.GetApprovedMrsById;

namespace PurchaseManagement.UnitTests.Application.Issue.Queries
{
    public sealed class GetApprovedMrsByIdQueryHandlerTests
    {
        private readonly Mock<IIssueQueryCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetApprovedMrsByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetApprovedMrsDetails(It.IsAny<string>()))
                .ReturnsAsync(new List<GetApprovedMrsByIdDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetApprovedMrsByIdDto>>(It.IsAny<object>()))
                .Returns(new List<GetApprovedMrsByIdDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetApprovedMrsByIdQuery { SearchPattern = "test" }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockRepo
                .Setup(r => r.GetApprovedMrsDetails(It.IsAny<string>()))
                .ReturnsAsync(new List<GetApprovedMrsByIdDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetApprovedMrsByIdDto>>(It.IsAny<object>()))
                .Returns(new List<GetApprovedMrsByIdDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetApprovedMrsByIdQuery { SearchPattern = "test" }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
