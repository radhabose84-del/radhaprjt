using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Commands.DeleteTripSheet;
using SalesManagement.Presentation.Validation.TripSheet;

namespace SalesManagement.UnitTests.Validators.TripSheet
{
    public sealed class DeleteTripSheetCommandValidatorTests
    {
        private readonly Mock<ITripSheetQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteTripSheetCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteTripSheetCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteTripSheetCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteTripSheetCommand(1));
            result.ShouldHaveAnyValidationError();
        }
    }
}
