using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.MiscMaster.Commands.DeleteMiscMaster;
using SalesManagement.Presentation.Validation.MiscMaster;

namespace SalesManagement.UnitTests.Validators.MiscMaster
{
    public sealed class DeleteMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMiscMasterCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidId_ExistingEntity_PassesValidation()
        {
            SetupHappyPath(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id / NotEmpty Rules ───────────────────────────────────────────────

        [Fact]
        public async Task Id_Zero_FailsNotEmpty()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Id is required.");
        }

        // ── NotFound Rules ────────────────────────────────────────────────────

        [Fact]
        public async Task Id_EntityNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand(999));

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Misc Master not found.");
        }

        [Fact]
        public async Task Id_EntityExists_PassesNotFoundCheck()
        {
            SetupHappyPath(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand(1));

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        // ── SoftDelete Rules ──────────────────────────────────────────────────

        [Fact]
        public async Task Id_ActiveRelationshipsExist_FailsSoftDeleteValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(2)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(2)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand(2));

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Cannot delete the relationship as it is active with another table.");
        }

        [Fact]
        public async Task Id_NoActiveRelationships_PassesSoftDeleteCheck()
        {
            SetupHappyPath(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
