using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Presentation.Validation.JournalMaster.AccountingPeriod;
using FinanceManagement.UnitTests.TestData;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.AccountingPeriod
{
    public sealed class UpdateAccountingPeriodCommandValidatorTests
    {
        private readonly Mock<IAccountingPeriodQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateAccountingPeriodCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.StatusExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(AccountingPeriodBuilders.ValidUpdateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NonExistentId_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(AccountingPeriodBuilders.ValidUpdateCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task Validate_IsActiveOutOfRange_Fails(int isActive)
        {
            SetupHappyPath();
            var cmd = AccountingPeriodBuilders.ValidUpdateCommand(isActive: isActive);

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_Fails(string? name)
        {
            SetupHappyPath();
            var cmd = AccountingPeriodBuilders.ValidUpdateCommand();
            cmd.PeriodName = name;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.PeriodName);
        }

        [Fact]
        public async Task Validate_EndDateBeforeStartDate_Fails()
        {
            SetupHappyPath();
            var cmd = AccountingPeriodBuilders.ValidUpdateCommand();
            cmd.StartDate = new DateOnly(2026, 6, 30);
            cmd.EndDate = new DateOnly(2026, 6, 1);

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.EndDate);
        }
    }
}
