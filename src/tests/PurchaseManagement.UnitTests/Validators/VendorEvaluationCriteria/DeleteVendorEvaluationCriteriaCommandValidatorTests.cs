using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.DeleteVendorEvaluationCriteria;
using PurchaseManagement.Presentation.Validation.VendorEvaluationCriteria;

namespace PurchaseManagement.UnitTests.Validators.VendorEvaluationCriteria
{
    public sealed class DeleteVendorEvaluationCriteriaCommandValidatorTests
    {
        private readonly Mock<IVendorEvaluationCriteriaQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteVendorEvaluationCriteriaCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteVendorEvaluationCriteriaCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteVendorEvaluationCriteriaCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteVendorEvaluationCriteriaCommand(99));

            result.ShouldHaveAnyValidationError();
        }
    }
}
