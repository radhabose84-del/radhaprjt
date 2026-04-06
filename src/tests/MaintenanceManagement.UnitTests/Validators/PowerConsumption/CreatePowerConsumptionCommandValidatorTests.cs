using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Power.PowerConsumption.Command.CreatePowerConsumption;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.Power.PowerConsumption;
using Xunit;

namespace MaintenanceManagement.UnitTests.Validators.PowerConsumption
{
    public sealed class CreatePowerConsumptionCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreatePowerConsumptionCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        private static CreatePowerConsumptionCommand ValidCommand() => new()
        {
            FeederTypeId = 1,
            FeederId = 1,
            UnitId = 1,
            OpeningReading = 100m,
            ClosingReading = 200m
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroFeederTypeId_FailsValidation()
        {
            var command = ValidCommand();
            command.FeederTypeId = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroFeederId_FailsValidation()
        {
            var command = ValidCommand();
            command.FeederId = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ClosingLessThanOpening_FailsValidation()
        {
            var command = ValidCommand();
            command.OpeningReading = 500m;
            command.ClosingReading = 100m;
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
