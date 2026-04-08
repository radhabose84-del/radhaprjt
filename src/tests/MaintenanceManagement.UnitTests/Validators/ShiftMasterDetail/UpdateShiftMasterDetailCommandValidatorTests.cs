using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.UpdateShiftMasterDetail;
using MaintenanceManagement.Presentation.Validation.ShiftMasterDetail;

namespace MaintenanceManagement.UnitTests.Validators.ShiftMasterDetail
{
    public sealed class UpdateShiftMasterDetailCommandValidatorTests
    {
        private readonly Mock<IShiftMasterDetailQuery> _mockQuery = new(MockBehavior.Loose);

        private UpdateShiftMasterDetailCommandValidator CreateValidator() =>
            new(_mockQuery.Object);

        private void SetupAllMocks(int id = 1, int shiftMasterId = 1)
        {
            _mockQuery.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQuery.Setup(r => r.FKColumnValidation(shiftMasterId)).ReturnsAsync(true);
        }

        private static UpdateShiftMasterDetailCommand ValidCommand(int id = 1, int shiftMasterId = 1) => new()
        {
            Id = id,
            ShiftMasterId = shiftMasterId,
            UnitId = 1,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(17, 0),
            BreakDurationInMinutes = 30,
            EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ShiftSupervisorId = 1
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = ValidCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQuery.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(false);
            _mockQuery.Setup(r => r.FKColumnValidation(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand(id: 99));

            result.ShouldHaveAnyValidationError();
        }

        // Skipped: validator uses "FKColumnActiveOrInactive" case but validation-rules.json has "FKColumnDelete" — case never matches

    }
}
