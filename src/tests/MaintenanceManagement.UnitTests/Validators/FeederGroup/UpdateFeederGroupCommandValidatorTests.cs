using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.UpdateFeederGroup;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.Power.FeederGroup;
using Xunit;

namespace MaintenanceManagement.UnitTests.Validators.FeederGroup
{
    public sealed class UpdateFeederGroupCommandValidatorTests
    {
        private readonly Mock<IFeederGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        // Note: constructor order is (MaxLengthProvider, IQueryRepo)
        private UpdateFeederGroupCommandValidator CreateValidator() =>
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
            var command = new UpdateFeederGroupCommand
            {
                Id = 1,
                FeederGroupCode = "FG001",
                FeederGroupName = "Updated Group",
                UnitId = 1,
                IsActive = 1
            };
            SetupAlreadyExists(command.FeederGroupCode, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = new UpdateFeederGroupCommand { Id = 1, FeederGroupCode = code, UnitId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
