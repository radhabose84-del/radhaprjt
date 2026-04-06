using FluentValidation.TestHelper;
using MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Presentation.Validation.ActivityMaster;
using MaintenanceManagement.Presentation.Validation.Common;

namespace MaintenanceManagement.UnitTests.Validators.ActivityMaster
{
    public sealed class UpdateActivityMasterCommandValidatorTests
    {
        private readonly Mock<IActivityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateActivityMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAlreadyExists(string name, int? id, bool exists = false)
        {
            _mockQueryRepo
                .Setup(r => r.GetByActivityNameAsync(name, id))
                .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateActivityMasterCommand
            {
                UpdateActivityMaster = new UpdateActivityMasterDto
                {
                    ActivityId = 1,
                    ActivityName = "Lubrication Check",
                    UnitId = 1,
                    DepartmentId = 1
                }
            };
            SetupAlreadyExists("Lubrication Check", 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyActivityName_FailsValidation(string? name)
        {
            var command = new UpdateActivityMasterCommand
            {
                UpdateActivityMaster = new UpdateActivityMasterDto { ActivityName = name }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateActivityName_FailsValidation()
        {
            var command = new UpdateActivityMasterCommand
            {
                UpdateActivityMaster = new UpdateActivityMasterDto
                {
                    ActivityId = 1,
                    ActivityName = "Lubrication Check",
                    UnitId = 1,
                    DepartmentId = 1
                }
            };
            SetupAlreadyExists("Lubrication Check", 1, exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
