using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IDocumentSequence;
using FinanceManagement.Application.DocumentSequence.Commands.DeleteDocumentSequence;
using FinanceManagement.Presentation.Validation.DocumentSequence;

namespace FinanceManagement.UnitTests.Validators.DocumentSequence
{
    public sealed class DeleteDocumentSequenceCommandValidatorTests
    {
        private readonly Mock<IDocumentSequenceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteDocumentSequenceCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteDocumentSequenceCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = new DeleteDocumentSequenceCommand(99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var command = new DeleteDocumentSequenceCommand(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
