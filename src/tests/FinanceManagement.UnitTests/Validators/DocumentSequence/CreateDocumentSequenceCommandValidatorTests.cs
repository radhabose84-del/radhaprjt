using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IDocumentSequence;
using FinanceManagement.Application.DocumentSequence.Commands.CreateDocumentSequence;
using FinanceManagement.Presentation.Validation.DocumentSequence;

namespace FinanceManagement.UnitTests.Validators.DocumentSequence
{
    public sealed class CreateDocumentSequenceCommandValidatorTests
    {
        private readonly Mock<IDocumentSequenceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateDocumentSequenceCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int typeId = 1, int fyId = 1, int docNo = 100)
        {
            _mockQueryRepo.Setup(r => r.TransactionTypeIdExistsAsync(typeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.FinancialYearExistsAsync(fyId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(typeId, fyId, docNo, null)).ReturnsAsync(false);
        }

        private static CreateDocumentSequenceCommand ValidCommand() =>
            new()
            {
                TransactionTypeId = 1,
                FinancialYearId = 1,
                DocNo = 100
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroTransactionTypeId_FailsValidation()
        {
            var command = ValidCommand();
            command.TransactionTypeId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TransactionTypeId);
        }

        [Fact]
        public async Task Validate_ZeroFinancialYearId_FailsValidation()
        {
            var command = ValidCommand();
            command.FinancialYearId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FinancialYearId);
        }

        [Fact]
        public async Task Validate_ZeroDocNo_FailsValidation()
        {
            var command = ValidCommand();
            command.DocNo = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DocNo);
        }

        [Fact]
        public async Task Validate_InvalidTransactionTypeId_FailsValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.TransactionTypeIdExistsAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TransactionTypeId);
        }

        [Fact]
        public async Task Validate_InvalidFinancialYearId_FailsValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.FinancialYearExistsAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FinancialYearId);
        }

        [Fact]
        public async Task Validate_DuplicateCompositeKey_FailsValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(1, 1, 100, null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DocNo);
        }

        [Fact]
        public async Task Validate_NegativeDocNo_FailsValidation()
        {
            var command = ValidCommand();
            command.DocNo = -1;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DocNo);
        }
    }
}
