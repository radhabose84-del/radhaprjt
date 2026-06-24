using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetAllRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetRecurringJournalTemplateById;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.RecurringJournalTemplate.Queries
{
    public sealed class GetAllRecurringJournalTemplateQueryHandlerTests
    {
        private readonly Mock<IRecurringJournalTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllRecurringJournalTemplateQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccessWithData()
        {
            var dtos = new List<RecurringJournalTemplateHeaderDto> { RecurringTemplateBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtos, 1));
            _mockMapper.Setup(m => m.Map<List<RecurringJournalTemplateHeaderDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetAllRecurringJournalTemplateQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }
    }

    public sealed class GetRecurringJournalTemplateByIdQueryHandlerTests
    {
        private readonly Mock<IRecurringJournalTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRecurringJournalTemplateByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDtoWithLines()
        {
            var dto = RecurringTemplateBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<RecurringJournalTemplateHeaderDto>(It.IsAny<object>())).Returns(dto);

            var result = await CreateSut().Handle(new GetRecurringJournalTemplateByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Lines.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((RecurringJournalTemplateHeaderDto?)null);

            var result = await CreateSut().Handle(new GetRecurringJournalTemplateByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
