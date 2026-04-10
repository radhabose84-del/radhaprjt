using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using MaintenanceManagement.Presentation.Validation.MiscTypeMaster;

namespace MaintenanceManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class DeleteMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupNotFound(int id, bool found)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(found);
        }

        private void SetupSoftDelete(int id, bool linked)
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(id)).ReturnsAsync(linked);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupNotFound(1, found: true);
            SetupSoftDelete(1, linked: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupNotFound(99, found: false);
            SetupSoftDelete(99, linked: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_Linked_FailsValidation()
        {
            SetupNotFound(1, found: true);
            SetupSoftDelete(1, linked: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand { Id = 1 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
