using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.CreateJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.DeleteJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.UpdateJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetAllJournalFlag;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetAllJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetJournalThresholdRuleById;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.JournalThresholdRule
{
    public sealed class JournalThresholdRuleHandlerTests
    {
        private readonly Mock<IJournalThresholdRuleCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IJournalThresholdRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        [Fact]
        public async Task Create_ReturnsSuccess_AndPublishesAudit()
        {
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.JournalThresholdRule>(It.IsAny<CreateJournalThresholdRuleCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.JournalThresholdRule());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.JournalThresholdRule>())).ReturnsAsync(7);

            var sut = new CreateJournalThresholdRuleCommandHandler(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);
            var result = await sut.Handle(JournalThresholdRuleBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(7);
            _mockMediator.Verify(m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "JOURNAL_THRESHOLD_CREATE"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsSuccess()
        {
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.JournalThresholdRule>(It.IsAny<UpdateJournalThresholdRuleCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.JournalThresholdRule());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.JournalThresholdRule>())).ReturnsAsync(1);

            var sut = new UpdateJournalThresholdRuleCommandHandler(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);
            var result = await sut.Handle(JournalThresholdRuleBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ReturnsTrue()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var sut = new DeleteJournalThresholdRuleCommandHandler(_mockCommandRepo.Object, _mockMediator.Object);
            var result = await sut.Handle(new DeleteJournalThresholdRuleCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetAll_ReturnsData()
        {
            var dtos = new List<JournalThresholdRuleDto> { JournalThresholdRuleBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtos, 1));
            _mockMapper.Setup(m => m.Map<List<JournalThresholdRuleDto>>(It.IsAny<object>())).Returns(dtos);

            var sut = new GetAllJournalThresholdRuleQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
            var result = await sut.Handle(new GetAllJournalThresholdRuleQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetById_NonExistent_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((JournalThresholdRuleDto?)null);

            var sut = new GetJournalThresholdRuleByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
            var result = await sut.Handle(new GetJournalThresholdRuleByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetFlags_ReturnsData()
        {
            var dtos = new List<JournalFlagDto> { JournalThresholdRuleBuilders.ValidFlagDto() };
            _mockQueryRepo.Setup(r => r.GetFlagsAsync(1, 10, null)).ReturnsAsync((dtos, 1));
            _mockMapper.Setup(m => m.Map<List<JournalFlagDto>>(It.IsAny<object>())).Returns(dtos);

            var sut = new GetAllJournalFlagQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
            var result = await sut.Handle(new GetAllJournalFlagQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }
    }
}
