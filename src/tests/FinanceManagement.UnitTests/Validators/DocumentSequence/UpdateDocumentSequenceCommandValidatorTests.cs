using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IDocumentSequence;
using FinanceManagement.Application.DocumentSequence.Commands.UpdateDocumentSequence;
using FinanceManagement.Presentation.Validation.DocumentSequence;

namespace FinanceManagement.UnitTests.Validators.DocumentSequence
{
    public sealed class UpdateDocumentSequenceCommandValidatorTests
    {
        private readonly Mock<IDocumentSequenceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateDocumentSequenceCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, int typeId = 1, int fyId = 1, int docNo = 100)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.TransactionTypeIdExistsAsync(typeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.FinancialYearExistsAsync(fyId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(typeId, fyId, docNo, id)).ReturnsAsync(false);
        }

        private static UpdateDocumentSequenceCommand ValidCommand() =>
            new()
            {
                Id = 1,
                TransactionTypeId = 1,
                FinancialYearId = 1,
                DocNo = 100,
                IsActive = 1
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
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = ValidCommand();
            command.Id = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ZeroTransactionTypeId_FailsValidation()
        {
            var command = ValidCommand();
            command.TransactionTypeId = 0;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TransactionTypeId);
        }

        [Fact]
        public async Task Validate_ZeroFinancialYearId_FailsValidation()
        {
            var command = ValidCommand();
            command.FinancialYearId = 0;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FinancialYearId);
        }

        [Fact]
        public async Task Validate_ZeroDocNo_FailsValidation()
        {
            var command = ValidCommand();
            command.DocNo = 0;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DocNo);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            var command = ValidCommand();
            command.IsActive = 5;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
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
        public async Task Validate_DuplicateCompositeKey_FailsValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(1, 1, 100, 1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DocNo);
        }
    }
}
