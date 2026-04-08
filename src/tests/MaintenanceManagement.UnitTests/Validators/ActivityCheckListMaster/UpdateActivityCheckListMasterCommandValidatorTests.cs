using FluentValidation.TestHelper;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.UpdateActivityCheckListMaster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MaintenanceManagement.Presentation.Validation.ActivityCheckListMaster;
using MaintenanceManagement.Presentation.Validation.Common;

namespace MaintenanceManagement.UnitTests.Validators.ActivityCheckListMaster
{
    public sealed class UpdateActivityCheckListMasterCommandValidatorTests
    {
        private readonly Mock<IActivityCheckListMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateActivityCheckListMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAlreadyExists(string name, int activityId, int id, bool exists = false)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsCheckListAsync(name, activityId, id))
                .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateActivityCheckListMasterCommand
            {
                Id = 1,
                ActivityChecklist = "Check lubrication",
                ActivityID = 1,
                UnitId = 1,
                IsActive = 1
            };
            SetupAlreadyExists(command.ActivityChecklist!, command.ActivityID, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyActivityChecklist_FailsValidation(string? name)
        {
            var command = new UpdateActivityCheckListMasterCommand
            {
                Id = 1,
                ActivityChecklist = name,
                ActivityID = 1,
                UnitId = 1,
                IsActive = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCheckList_FailsValidation()
        {
            var command = new UpdateActivityCheckListMasterCommand
            {
                Id = 1,
                ActivityChecklist = "Check lubrication",
                ActivityID = 1,
                UnitId = 1,
                IsActive = 1
            };
            SetupAlreadyExists(command.ActivityChecklist!, command.ActivityID, command.Id, exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
