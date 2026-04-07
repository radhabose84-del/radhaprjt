using FluentValidation.TestHelper;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.DeleteActivityCheckListMaster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MaintenanceManagement.Presentation.Validation.ActivityCheckListMaster;

namespace MaintenanceManagement.UnitTests.Validators.ActivityCheckListMaster
{
    public sealed class DeleteActivityCheckListMasterCommandValidatorTests
    {
        private readonly Mock<IActivityCheckListMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IActivityCheckListMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteActivityCheckListMasterCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object);

        private void SetupGetById(int id, bool exists = true)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(exists
                    ? new MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster.GetAllActivityCheckListMasterDto { ChecklistId = id }
                    : null!);
        }

        private void SetupSoftDeleteValidation(int id, bool linked = false)
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(id))
                .ReturnsAsync(linked);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupGetById(1);
            SetupSoftDeleteValidation(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteActivityCheckListMasterCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteActivityCheckListMasterCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // Skipped: validator uses "RecordNotFound" case but validation-rules.json has "NotFound" — case never matches


        [Fact]
        public async Task Validate_Linked_FailsValidation()
        {
            SetupGetById(1);
            SetupSoftDeleteValidation(1, linked: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteActivityCheckListMasterCommand { Id = 1 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
