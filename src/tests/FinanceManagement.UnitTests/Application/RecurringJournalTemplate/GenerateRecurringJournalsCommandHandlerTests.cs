using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.GenerateRecurringJournals;

namespace FinanceManagement.UnitTests.Application.RecurringJournalTemplate
{
    public sealed class GenerateRecurringJournalsCommandHandlerTests
    {
        private readonly Mock<IRecurringJournalGenerationService> _service = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);

        private GenerateRecurringJournalsCommandHandler CreateSut() => new(_service.Object, _ip.Object);

        [Fact]
        public async Task Handle_RunsGenerationForSessionCompany_ReturnsCount()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(1);
            _service.Setup(s => s.GenerateForPeriodAsync(1, 101, "2026-06", It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            var result = await CreateSut().Handle(
                new GenerateRecurringJournalsCommand { BaseCurrencyId = 101, Period = "2026-06", VoucherDate = new DateOnly(2026, 6, 1) },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(2);
            _service.Verify(s => s.GenerateForPeriodAsync(1, 101, "2026-06", It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoSessionCompany_Throws()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns((int?)null);

            var act = async () => await CreateSut().Handle(
                new GenerateRecurringJournalsCommand { BaseCurrencyId = 101, Period = "2026-06", VoucherDate = new DateOnly(2026, 6, 1) },
                CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
