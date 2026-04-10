using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailAutoComplete;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalStepDetail.Queries
{
    public sealed class GetApprovalStepDetailAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IApprovalStepDetailQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILookupRepository> _mockLookupRepo = new(MockBehavior.Loose);

        private GetApprovalStepDetailAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockLookupRepo.Object);

        [Fact]
        public async Task Handle_ReturnsMappedList()
        {
            var entities = new List<BackgroundService.Domain.Entities.Workflow.ApprovalStepDetail>
            {
                new() { Id = 1 }
            };

            _mockQueryRepo
                .Setup(r => r.GetApprovalStepDetailAutoComplete(It.IsAny<string>()))
                .ReturnsAsync(entities);

            var dtos = new List<ApprovalStepDetailAutoCompleteDto>
            {
                new() { Id = 1, MenuId = 100, TargetValueId = 10 }
            };

            _mockMapper
                .Setup(m => m.Map<List<ApprovalStepDetailAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockLookupRepo
                .Setup(r => r.GetMenuNamesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string> { { 100, "Test Menu" } });

            _mockLookupRepo
                .Setup(r => r.GetUserNamesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string> { { 10, "Test User" } });

            var sut = CreateSut();

            var result = await sut.Handle(
                new GetApprovalStepDetailAutoCompleteQuery { SearchPattern = "test" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptySearch_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetApprovalStepDetailAutoComplete(string.Empty))
                .ReturnsAsync(new List<BackgroundService.Domain.Entities.Workflow.ApprovalStepDetail>());

            _mockMapper
                .Setup(m => m.Map<List<ApprovalStepDetailAutoCompleteDto>>(
                    It.IsAny<List<BackgroundService.Domain.Entities.Workflow.ApprovalStepDetail>>()))
                .Returns(new List<ApprovalStepDetailAutoCompleteDto>());

            _mockLookupRepo
                .Setup(r => r.GetMenuNamesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string>());

            _mockLookupRepo
                .Setup(r => r.GetUserNamesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string>());

            var sut = CreateSut();

            var result = await sut.Handle(
                new GetApprovalStepDetailAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
