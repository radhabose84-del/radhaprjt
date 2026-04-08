using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.DeletePurchaseIndent;
using PurchaseManagement.Presentation.Validation.PurchaseIndent;

namespace PurchaseManagement.UnitTests.Validators.PurchaseIndent
{
    public sealed class DeletePurchaseIndentCommandValidatorTests
    {
        private readonly Mock<IPurchaseIndentQuery> _mockRepo = new(MockBehavior.Loose);

        private DeletePurchaseIndentCommandValidator CreateValidator() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeletePurchaseIndentCommand { Id = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = new DeletePurchaseIndentCommand { Id = 99 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
