using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.UpdateFeeder;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.Power.Feeder;
using Xunit;

namespace MaintenanceManagement.UnitTests.Validators.Feeder
{
    public sealed class UpdateFeederCommandValidatorTests
    {
        private readonly Mock<IFeederQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateFeederCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupAlreadyExists(string? code, int id, bool exists = false)
        {
            if (code != null)
                _mockQueryRepo
                    .Setup(r => r.AlreadyExistsAsync(code, id))
                    .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateFeederCommand
            {
                Id = 1,
                FeederCode = "FDR001",
                FeederName = "Updated Feeder",
                FeederTypeId = 1,
                UnitId = 1,
                IsActive = 1
            };
            SetupAlreadyExists(command.FeederCode, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyFeederCode_FailsValidation(string? code)
        {
            var command = new UpdateFeederCommand { Id = 1, FeederCode = code, FeederName = "Test", UnitId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = new UpdateFeederCommand { Id = 1, FeederCode = "FDR001", FeederName = "Test", UnitId = 1 };
            SetupAlreadyExists(command.FeederCode, command.Id, exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
