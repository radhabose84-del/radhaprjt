using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Power.GeneratorConsumption.Command;
using MaintenanceManagement.Presentation.Validation.Power.GeneratorConsumption;
using Xunit;

namespace MaintenanceManagement.UnitTests.Validators.GeneratorConsumption
{
    public sealed class CreateGeneratorConsumptionCommandValidatorTests
    {
        private CreateGeneratorConsumptionCommandValidator CreateValidator() => new();

        private static CreateGeneratorConsumptionCommand ValidCommand() => new()
        {
            GeneratorId = 1,
            StartTime = DateTimeOffset.UtcNow.AddHours(-2),
            EndTime = DateTimeOffset.UtcNow,
            DieselConsumption = 10m,
            OpeningEnergyReading = 100m,
            ClosingEnergyReading = 200m,
            PurposeId = 1,
            UnitId = 1
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroGeneratorId_FailsValidation()
        {
            var command = ValidCommand();
            command.GeneratorId = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ClosingLessThanOpening_FailsValidation()
        {
            var command = ValidCommand();
            command.OpeningEnergyReading = 500m;
            command.ClosingEnergyReading = 100m;
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EndTimeBeforeStartTime_FailsValidation()
        {
            var command = ValidCommand();
            command.StartTime = DateTimeOffset.UtcNow;
            command.EndTime = DateTimeOffset.UtcNow.AddHours(-1);
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
