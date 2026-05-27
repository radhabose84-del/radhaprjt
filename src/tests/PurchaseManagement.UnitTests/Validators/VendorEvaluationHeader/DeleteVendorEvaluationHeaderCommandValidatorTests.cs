using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.DeleteVendorEvaluationHeader;
using PurchaseManagement.Presentation.Validation.VendorEvaluationHeader;

namespace PurchaseManagement.UnitTests.Validators.VendorEvaluationHeader
{
    public sealed class DeleteVendorEvaluationHeaderCommandValidatorTests
    {
        private readonly Mock<IVendorEvaluationHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteVendorEvaluationHeaderCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteVendorEvaluationHeaderCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteVendorEvaluationHeaderCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteVendorEvaluationHeaderCommand(99));

            result.ShouldHaveAnyValidationError();
        }
    }
}
