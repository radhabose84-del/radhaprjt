using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Presentation.Validation.JournalMaster.Journal;
using FinanceManagement.UnitTests.TestData;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.Journal
{
    public sealed class UpdateJournalCommandValidatorTests
    {
        private readonly Mock<IJournalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private UpdateJournalCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockIp.Object);

        private void SetupHappyPath()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetOpenPeriodByDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(((int, int)?)(4, 3));
            _mockQueryRepo.Setup(r => r.VoucherTypeExistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GlAccountExistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetCostCentreMandatoryAccountIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(Array.Empty<int>());
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsManualDraftAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsPotentialDuplicateAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<decimal>(), It.IsAny<decimal>(),
                It.IsAny<IReadOnlyList<(int, decimal, decimal)>>(), It.IsAny<int?>())).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_DuplicateVoucher_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.IsPotentialDuplicateAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<decimal>(), It.IsAny<decimal>(),
                It.IsAny<IReadOnlyList<(int, decimal, decimal)>>(), It.IsAny<int?>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(JournalBuilders.ValidUpdateCommand());

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("already exists"));
        }

        [Fact]
        public async Task Validate_DuplicateVoucher_WithOverride_Passes()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.IsPotentialDuplicateAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<decimal>(), It.IsAny<decimal>(),
                It.IsAny<IReadOnlyList<(int, decimal, decimal)>>(), It.IsAny<int?>())).ReturnsAsync(true);

            var cmd = JournalBuilders.ValidUpdateCommand();
            cmd.OverrideDuplicate = true;

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(JournalBuilders.ValidUpdateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NotManualDraft_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.IsManualDraftAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(JournalBuilders.ValidUpdateCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_BalancedInTxnButNotInBaseCurrency_Fails()
        {
            SetupHappyPath();
            var cmd = JournalBuilders.ValidUpdateCommand(lines: new List<JournalLineInputDto>
            {
                new() { GlAccountId = 5200101, DrAmount = 5000m, CrAmount = 0m, CurrencyId = 1, ExchangeRate = 1m, CostCentreId = 1, ProfitCentreId = 1 },
                new() { GlAccountId = 2200101, DrAmount = 0m, CrAmount = 5000m, CurrencyId = 1, ExchangeRate = 2m, CostCentreId = null, ProfitCentreId = 1 }
            });

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("base currency"));
        }

        [Fact]
        public async Task Validate_BalancedInBaseCurrency_Passes()
        {
            SetupHappyPath();
            var cmd = JournalBuilders.ValidUpdateCommand(lines: new List<JournalLineInputDto>
            {
                new() { GlAccountId = 5200101, DrAmount = 5000m, CrAmount = 0m, CurrencyId = 1, ExchangeRate = 2m, CostCentreId = 1, ProfitCentreId = 1 },
                new() { GlAccountId = 2200101, DrAmount = 0m, CrAmount = 5000m, CurrencyId = 1, ExchangeRate = 2m, CostCentreId = null, ProfitCentreId = 1 }
            });

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
