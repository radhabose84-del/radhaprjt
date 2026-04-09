using AutoMapper;
using BackgroundService.Application.Workflow.ApprovalRules.Queries.GetByIdApprovalRule;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalRule.Queries
{
    public sealed class GetByIdApprovalRuleQueryHandlerTests
    {
        private readonly Mock<IApprovalRuleQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetByIdApprovalRuleQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var entity = new BackgroundService.Domain.Entities.Workflow.ApprovalRule { Id = 1 };
            var dto = new ApprovalRuleByIdDto { Id = 1, ActionId = 1, Priority = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<ApprovalRuleByIdDto>(entity))
                .Returns(dto);

            var sut = CreateSut();

            var result = await sut.Handle(
                new GetByIdApprovalRuleQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_CallsGetByIdOnce()
        {
            var entity = new BackgroundService.Domain.Entities.Workflow.ApprovalRule { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<ApprovalRuleByIdDto>(entity))
                .Returns(new ApprovalRuleByIdDto { Id = 1 });

            var sut = CreateSut();

            await sut.Handle(new GetByIdApprovalRuleQuery { Id = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}
