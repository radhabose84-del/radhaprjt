using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Presentation.Validation.JournalMaster.AccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.DeleteAccountingPeriod;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.AccountingPeriod
{
    public sealed class DeleteAccountingPeriodCommandValidatorTests
    {
        private readonly Mock<IAccountingPeriodQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteAccountingPeriodCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ExistingId_Passes()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteAccountingPeriodCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_Fails()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteAccountingPeriodCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NonExistentId_Fails()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteAccountingPeriodCommand(99));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
