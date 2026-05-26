using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IMiscTypeMaster;
using QCManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster;
using QCManagement.Presentation.Validation.MiscTypeMaster;

namespace QCManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class DeleteMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMiscTypeMasterCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_ExistingEntity_PassesValidation()
        {
            SetupHappyPath(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Id_Zero_FailsNotEmpty()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Id is required.");
        }

        [Fact]
        public async Task Id_EntityNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(999));

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Misc Type Master not found.");
        }

        [Fact]
        public async Task Id_EntityExists_PassesNotFoundCheck()
        {
            SetupHappyPath(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(1));

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Id_ActiveRelationshipsExist_FailsSoftDeleteValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(2)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(2)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(2));

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Cannot delete the relationship as it is active with another table.");
        }
    }
}
