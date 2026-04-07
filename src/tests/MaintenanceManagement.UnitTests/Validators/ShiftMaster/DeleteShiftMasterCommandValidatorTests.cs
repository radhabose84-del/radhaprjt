using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.DeleteShiftMaster;
using MaintenanceManagement.Presentation.Validation.ShiftMaster;

namespace MaintenanceManagement.UnitTests.Validators.ShiftMaster
{
    public sealed class DeleteShiftMasterCommandValidatorTests
    {
        private readonly Mock<IShiftMasterQuery> _mockQuery = new(MockBehavior.Loose);

        private DeleteShiftMasterCommandValidator CreateValidator() =>
            new(_mockQuery.Object);

        private void SetupNotFound(int id, bool found)
        {
            _mockQuery.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(found);
        }

        private void SetupSoftDelete(int id, bool linked)
        {
            _mockQuery.Setup(r => r.SoftDeleteValidation(id)).ReturnsAsync(linked);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupNotFound(1, found: true);
            SetupSoftDelete(1, linked: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteShiftMasterCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteShiftMasterCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupNotFound(99, found: false);
            SetupSoftDelete(99, linked: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteShiftMasterCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_Linked_FailsValidation()
        {
            SetupNotFound(1, found: true);
            SetupSoftDelete(1, linked: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteShiftMasterCommand { Id = 1 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
