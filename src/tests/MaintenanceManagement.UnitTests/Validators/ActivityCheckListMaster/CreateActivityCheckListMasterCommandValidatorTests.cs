using FluentValidation.TestHelper;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.CreateActivityCheckListMaster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MaintenanceManagement.Presentation.Validation.ActivityCheckListMaster;
using MaintenanceManagement.Presentation.Validation.Common;

namespace MaintenanceManagement.UnitTests.Validators.ActivityCheckListMaster
{
    public sealed class CreateActivityCheckListMasterCommandValidatorTests
    {
        private readonly Mock<IActivityCheckListMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateActivityCheckListMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAlreadyExists(string name, int activityId, bool exists = false)
        {
            _mockQueryRepo
                .Setup(r => r.GetByActivityCheckListAsync(name, activityId))
                .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateActivityCheckListMasterCommand
            {
                ActivityCheckList = "Check lubrication",
                ActivityID = 1,
                UnitId = 1
            };
            SetupAlreadyExists(command.ActivityCheckList!, command.ActivityID);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyActivityCheckList_FailsValidation(string? name)
        {
            var command = new CreateActivityCheckListMasterCommand
            {
                ActivityCheckList = name,
                ActivityID = 1,
                UnitId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCheckList_FailsValidation()
        {
            var command = new CreateActivityCheckListMasterCommand
            {
                ActivityCheckList = "Check lubrication",
                ActivityID = 1,
                UnitId = 1
            };
            SetupAlreadyExists(command.ActivityCheckList!, command.ActivityID, exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
