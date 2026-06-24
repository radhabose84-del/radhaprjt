using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAccountingPeriodById;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.AccountingPeriod.Queries
{
    public sealed class GetAccountingPeriodByIdQueryHandlerTests
    {
        private readonly Mock<IAccountingPeriodQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAccountingPeriodByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = AccountingPeriodBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<AccountingPeriodDto>(It.IsAny<object>())).Returns(dto);

            var result = await CreateSut().Handle(new GetAccountingPeriodByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((AccountingPeriodDto?)null);

            var result = await CreateSut().Handle(new GetAccountingPeriodByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
