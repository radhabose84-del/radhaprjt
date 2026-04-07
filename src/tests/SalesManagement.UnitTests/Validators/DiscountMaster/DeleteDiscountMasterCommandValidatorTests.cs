using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.DeleteDiscountMaster;
using SalesManagement.Presentation.Validation.DiscountMaster;

namespace SalesManagement.UnitTests.Validators.DiscountMaster
{
    public sealed class DeleteDiscountMasterCommandValidatorTests
    {
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteDiscountMasterCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(new DeleteDiscountMasterCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id / NotEmpty Rules ───────────────────────────────────────────────

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteDiscountMasterCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── NotFound Rules ────────────────────────────────────────────────────

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteDiscountMasterCommand(1));

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task EntityExists_PassesNotFoundCheck()
        {
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(new DeleteDiscountMasterCommand(1));

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
