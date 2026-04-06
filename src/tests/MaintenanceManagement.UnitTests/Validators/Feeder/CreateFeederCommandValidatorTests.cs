using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.CreateFeeder;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.Power.Feeder;
using Xunit;

namespace MaintenanceManagement.UnitTests.Validators.Feeder
{
    public sealed class CreateFeederCommandValidatorTests
    {
        private readonly Mock<IFeederQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateFeederCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAlreadyExists(string? code, int unitId, bool exists = false)
        {
            if (code != null)
                _mockQueryRepo
                    .Setup(r => r.AlreadyExistsAsync(code, unitId))
                    .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateFeederCommand
            {
                FeederCode = "FDR001",
                FeederName = "Test Feeder",
                UnitId = 1
            };
            SetupAlreadyExists(command.FeederCode, command.UnitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyFeederCode_FailsValidation(string? code)
        {
            var command = new CreateFeederCommand { FeederCode = code, FeederName = "Test", UnitId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = new CreateFeederCommand { FeederCode = "FDR001", FeederName = "Test", UnitId = 1 };
            SetupAlreadyExists(command.FeederCode, command.UnitId, exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
