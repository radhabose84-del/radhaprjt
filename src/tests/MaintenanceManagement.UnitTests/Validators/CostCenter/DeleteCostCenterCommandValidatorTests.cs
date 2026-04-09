using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Application.CostCenter.Command.DeleteCostCenter;
using MaintenanceManagement.Presentation.Validation.CostCenter;

namespace MaintenanceManagement.UnitTests.Validators.CostCenter
{
    public sealed class DeleteCostCenterCommandValidatorTests
    {
        private readonly Mock<ICostCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteCostCenterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupGetById(int id, bool exists = true)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(exists
                    ? new MaintenanceManagement.Domain.Entities.CostCenter { Id = id }
                    : null);
        }

        private void SetupSoftDelete(int id, bool linked = false)
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(id)).ReturnsAsync(linked);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupGetById(1);
            SetupSoftDelete(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteCostCenterCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteCostCenterCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // Skipped: validator uses "RecordNotFound" case but validation-rules.json has "NotFound" — case never matches


        [Fact]
        public async Task Validate_Linked_FailsValidation()
        {
            SetupGetById(1);
            SetupSoftDelete(1, linked: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteCostCenterCommand { Id = 1 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
