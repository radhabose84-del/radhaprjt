using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Presentation.Validation.JournalMaster.AccountingPeriod;
using FinanceManagement.UnitTests.TestData;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.AccountingPeriod
{
    public sealed class CreateAccountingPeriodCommandValidatorTests
    {
        private readonly Mock<IAccountingPeriodQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateAccountingPeriodCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockIp.Object);

        private void SetupHappyPath()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.StatusExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(AccountingPeriodBuilders.ValidCreateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_Fails(string? name)
        {
            SetupHappyPath();
            var cmd = AccountingPeriodBuilders.ValidCreateCommand();
            cmd.PeriodName = name;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.PeriodName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        public async Task Validate_PeriodNoOutOfRange_Fails(int periodNo)
        {
            SetupHappyPath();
            var cmd = AccountingPeriodBuilders.ValidCreateCommand(periodNo: periodNo);

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.PeriodNo);
        }

        [Fact]
        public async Task Validate_EndDateBeforeStartDate_Fails()
        {
            SetupHappyPath();
            var cmd = AccountingPeriodBuilders.ValidCreateCommand();
            cmd.StartDate = new DateOnly(2026, 6, 30);
            cmd.EndDate = new DateOnly(2026, 6, 1);

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.EndDate);
        }

        [Fact]
        public async Task Validate_DuplicatePeriod_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(1, 3, 3, It.IsAny<int?>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(AccountingPeriodBuilders.ValidCreateCommand());
            result.ShouldHaveValidationErrorFor(x => x.PeriodNo);
        }

        [Fact]
        public async Task Validate_NonExistentStatus_Fails()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.StatusExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(AccountingPeriodBuilders.ValidCreateCommand());
            result.ShouldHaveValidationErrorFor(x => x.StatusId);
        }
    }
}
