using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Commands.DeleteTransactionTypeMaster;
using FinanceManagement.Presentation.Validation.TransactionTypeMaster;

namespace FinanceManagement.UnitTests.Validators.TransactionTypeMaster
{
    public sealed class DeleteTransactionTypeMasterCommandValidatorTests
    {
        private readonly Mock<ITransactionTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteTransactionTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteTransactionTypeMasterCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = new DeleteTransactionTypeMasterCommand(99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var command = new DeleteTransactionTypeMasterCommand(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
