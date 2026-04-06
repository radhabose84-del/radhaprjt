using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.SalesContact.Commands.DeleteSalesContact;
using SalesManagement.Presentation.Validation.SalesContact;

namespace SalesManagement.UnitTests.Validators.SalesContact
{
    public sealed class DeleteSalesContactCommandValidatorTests
    {
        private readonly Mock<ISalesContactQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteSalesContactCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteSalesContactCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteSalesContactCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteSalesContactCommand(1));
            result.ShouldHaveAnyValidationError();
        }
    }
}
