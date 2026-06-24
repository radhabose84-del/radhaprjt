using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetJournalById;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.Journal.Queries
{
    public sealed class GetJournalByIdQueryHandlerTests
    {
        private readonly Mock<IJournalQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetJournalByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDtoWithLines()
        {
            var dto = JournalBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<JournalHeaderDto>(It.IsAny<object>())).Returns(dto);

            var result = await CreateSut().Handle(new GetJournalByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Lines.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((JournalHeaderDto?)null);

            var result = await CreateSut().Handle(new GetJournalByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
