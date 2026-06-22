using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Commands.DeleteSalesAgreement;
using SalesManagement.Presentation.Validation.SalesAgreement;

namespace SalesManagement.UnitTests.Validators.SalesAgreement
{
    public sealed class DeleteSalesAgreementCommandValidatorTests
    {
        private readonly Mock<ISalesAgreementQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteSalesAgreementCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteSalesAgreementCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteSalesAgreementCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteSalesAgreementCommand(1));
            result.ShouldHaveAnyValidationError();
        }
    }
}
