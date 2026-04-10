using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.DeleteMachineGroup;
using MaintenanceManagement.Presentation.Validation.MachineGroup;

namespace MaintenanceManagement.UnitTests.Validators.MachineGroup
{
    public sealed class DeleteMachineGroupCommandValidatorTests
    {
        private readonly Mock<IMachineGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteMachineGroupCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupNotFound(int id, bool exists)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(exists);
        }

        private void SetupSoftDelete(int id, bool linked)
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(linked);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupNotFound(1, exists: true);
            SetupSoftDelete(1, linked: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMachineGroupCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteMachineGroupCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupNotFound(99, exists: false);
            SetupSoftDelete(99, linked: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMachineGroupCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_Linked_FailsValidation()
        {
            SetupNotFound(1, exists: true);
            SetupSoftDelete(1, linked: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMachineGroupCommand { Id = 1 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
