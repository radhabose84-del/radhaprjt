using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.DeleteVendorRatingGrade;
using PurchaseManagement.Presentation.Validation.VendorRatingGrade;

namespace PurchaseManagement.UnitTests.Validators.VendorRatingGrade
{
    public sealed class DeleteVendorRatingGradeCommandValidatorTests
    {
        private readonly Mock<IVendorRatingGradeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteVendorRatingGradeCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteVendorRatingGradeCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteVendorRatingGradeCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteVendorRatingGradeCommand(99));

            result.ShouldHaveAnyValidationError();
        }
    }
}
