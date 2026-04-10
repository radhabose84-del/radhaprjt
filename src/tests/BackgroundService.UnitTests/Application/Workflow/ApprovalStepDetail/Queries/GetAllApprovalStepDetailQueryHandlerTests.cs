using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetAllApprovalStepDetail;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using MediatR;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalStepDetail.Queries
{
    public sealed class GetAllApprovalStepDetailQueryHandlerTests
    {
        private readonly Mock<IApprovalStepDetailQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILookupRepository> _mockLookupRepo = new(MockBehavior.Loose);

        private GetAllApprovalStepDetailQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockLookupRepo.Object);

        private void SetupHappyPath(int count = 1)
        {
            var entities = new List<BackgroundService.Domain.Entities.Workflow.ApprovalStepDetail>();
            for (int i = 0; i < count; i++)
                entities.Add(new BackgroundService.Domain.Entities.Workflow.ApprovalStepDetail { Id = i + 1 });

            _mockQueryRepo
                .Setup(r => r.GetAllApprovalStepDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((entities, count));

            var dtos = new List<ApprovalStepDetailDto>();
            for (int i = 0; i < count; i++)
                dtos.Add(new ApprovalStepDetailDto { Id = i + 1, MenuId = 100 });

            _mockMapper
                .Setup(m => m.Map<List<ApprovalStepDetailDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Workflow.ApprovalStepDetail>>()))
                .Returns(dtos);

            _mockLookupRepo
                .Setup(r => r.GetMenuNamesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string> { { 100, "Test Menu" } });
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            var result = await sut.Handle(
                new GetAllApprovalStepDetailQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            SetupHappyPath(0);
            var sut = CreateSut();

            var result = await sut.Handle(
                new GetAllApprovalStepDetailQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            var result = await sut.Handle(
                new GetAllApprovalStepDetailQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }
    }
}
