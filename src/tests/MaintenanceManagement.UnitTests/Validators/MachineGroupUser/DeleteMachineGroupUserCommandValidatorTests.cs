using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Command.DeleteMachineGroupUser;
using MaintenanceManagement.Presentation.Validation.MachineGroupUser;

namespace MaintenanceManagement.UnitTests.Validators.MachineGroupUser
{
    public sealed class DeleteMachineGroupUserCommandValidatorTests
    {
        private readonly Mock<IMachineGroupUserQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMachineGroupUserCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupNotFound(int id, bool notFound)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(notFound);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupNotFound(1, notFound: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMachineGroupUserCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteMachineGroupUserCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupNotFound(99, notFound: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMachineGroupUserCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
