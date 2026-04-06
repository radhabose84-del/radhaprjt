using FluentValidation.TestHelper;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.CreateFeederGroup;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.Power.FeederGroup;
using Xunit;

namespace MaintenanceManagement.UnitTests.Validators.FeederGroup
{
    public sealed class CreateFeederGroupCommandValidatorTests
    {
        private readonly Mock<IFeederGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private CreateFeederGroupCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object, _mockIpService.Object);

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
            var command = new CreateFeederGroupCommand
            {
                FeederGroupCode = "FG001",
                FeederGroupName = "Test Group",
                UnitId = 1
            };
            SetupAlreadyExists(command.FeederGroupCode, command.UnitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = new CreateFeederGroupCommand { FeederGroupCode = code, FeederGroupName = "Test", UnitId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = new CreateFeederGroupCommand { FeederGroupCode = "FG001", FeederGroupName = "Test", UnitId = 1 };
            SetupAlreadyExists(command.FeederGroupCode, command.UnitId, exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
