using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.ApprovalRules.Queries.GetAllApprovalRule;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using Contracts.Common;
using MediatR;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalRule.Queries
{
    public sealed class GetAllApprovalRuleQueryHandlerTests
    {
        private readonly Mock<IApprovalRuleQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILookupRepository> _mockLookupRepo = new(MockBehavior.Loose);

        private GetAllApprovalRuleQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockLookupRepo.Object);

        private void SetupHappyPath(int count = 1)
        {
            var entities = new List<BackgroundService.Domain.Entities.Workflow.ApprovalRule>();
            for (int i = 0; i < count; i++)
                entities.Add(new BackgroundService.Domain.Entities.Workflow.ApprovalRule { Id = i + 1 });

            _mockQueryRepo
                .Setup(r => r.GetAllApprovalRuleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((entities, count));

            var dtos = new List<ApprovalRuleDto>();
            for (int i = 0; i < count; i++)
                dtos.Add(new ApprovalRuleDto { Id = i + 1, MenuId = 100 });

            _mockMapper
                .Setup(m => m.Map<List<ApprovalRuleDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Workflow.ApprovalRule>>()))
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
                new GetAllApprovalRuleQuery { PageNumber = 1, PageSize = 15 },
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
                new GetAllApprovalRuleQuery { PageNumber = 1, PageSize = 15 },
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
                new GetAllApprovalRuleQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }
    }
}
