using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.DeleteShiftMasterDetail;
using MaintenanceManagement.Presentation.Validation.ShiftMasterDetail;

namespace MaintenanceManagement.UnitTests.Validators.ShiftMasterDetail
{
    public sealed class DeleteShiftMasterDetailCommandValidatorTests
    {
        private readonly Mock<IShiftMasterDetailQuery> _mockQuery = new(MockBehavior.Loose);

        private DeleteShiftMasterDetailCommandValidator CreateValidator() =>
            new(_mockQuery.Object);

        private void SetupNotFound(int id, bool found)
        {
            _mockQuery.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(found);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupNotFound(1, found: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteShiftMasterDetailCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteShiftMasterDetailCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupNotFound(99, found: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteShiftMasterDetailCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
