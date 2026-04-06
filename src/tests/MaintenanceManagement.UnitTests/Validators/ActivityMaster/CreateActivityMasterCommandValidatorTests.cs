using FluentValidation.TestHelper;
using MaintenanceManagement.Application.ActivityMaster.Command.CreateActivityMaster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Presentation.Validation.ActivityMaster;
using MaintenanceManagement.Presentation.Validation.Common;

namespace MaintenanceManagement.UnitTests.Validators.ActivityMaster
{
    public sealed class CreateActivityMasterCommandValidatorTests
    {
        private readonly Mock<IActivityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateActivityMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAlreadyExists(string name, bool exists = false)
        {
            _mockQueryRepo
                .Setup(r => r.GetByActivityNameAsync(name, null))
                .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateActivityMasterCommand
            {
                CreateActivityMasterDto = new CreateActivityMasterDto
                {
                    ActivityName = "Lubrication Check",
                    UnitId = 1,
                    DepartmentId = 1
                }
            };
            SetupAlreadyExists("Lubrication Check");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyActivityName_FailsValidation(string? name)
        {
            var command = new CreateActivityMasterCommand
            {
                CreateActivityMasterDto = new CreateActivityMasterDto { ActivityName = name }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateActivityName_FailsValidation()
        {
            var command = new CreateActivityMasterCommand
            {
                CreateActivityMasterDto = new CreateActivityMasterDto
                {
                    ActivityName = "Lubrication Check",
                    UnitId = 1,
                    DepartmentId = 1
                }
            };
            SetupAlreadyExists("Lubrication Check", exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
