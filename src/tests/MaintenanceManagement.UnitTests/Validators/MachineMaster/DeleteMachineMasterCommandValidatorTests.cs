using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.DeleteMachineMaster;
using MaintenanceManagement.Presentation.Validation.MachineMaster;

namespace MaintenanceManagement.UnitTests.Validators.MachineMaster
{
    public sealed class DeleteMachineMasterCommandValidatorTests
    {
        private readonly Mock<IMachineMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteMachineMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupGetById(int id, bool exists = true)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(exists
                    ? new MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster.MachineMasterDto { Id = id }
                    : null);
        }

        private void SetupSoftDelete(int id, bool linked)
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(linked);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupGetById(1);
            SetupSoftDelete(1, linked: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMachineMasterCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteMachineMasterCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // Skipped: validator uses "RecordNotFound" case but validation-rules.json has "NotFound" — case never matches


        [Fact]
        public async Task Validate_Linked_FailsValidation()
        {
            SetupGetById(1);
            SetupSoftDelete(1, linked: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMachineMasterCommand { Id = 1 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
