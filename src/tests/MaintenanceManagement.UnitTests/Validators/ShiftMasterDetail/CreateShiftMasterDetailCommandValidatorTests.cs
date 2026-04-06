using FluentValidation.TestHelper;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.CreateShiftMasterDetail;
using MaintenanceManagement.Presentation.Validation.ShiftMasterDetail;

namespace MaintenanceManagement.UnitTests.Validators.ShiftMasterDetail
{
    public sealed class CreateShiftMasterDetailCommandValidatorTests
    {
        private CreateShiftMasterDetailCommandValidator CreateValidator() => new();

        private static CreateShiftMasterDetailCommand ValidCommand() => new()
        {
            ShiftMasterId = 1,
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
            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroShiftMasterId_FailsValidation()
        {
            var command = ValidCommand();
            command.ShiftMasterId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.UnitId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DefaultEffectiveDate_FailsValidation()
        {
            var command = ValidCommand();
            command.EffectiveDate = default;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
