using BudgetManagement.Application.BudgetGroup.Command.DeleteBudgetGroup;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.Presentation.Validation.BudgetGroup;
using FluentValidation.TestHelper;

namespace BudgetManagement.UnitTests.Validators.BudgetGroup
{
    public sealed class DeleteBudgetGroupCommandValidatorTests
    {
        private readonly Mock<IBudgetGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteBudgetGroupCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupExists(int id, bool exists = true)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(exists
                    ? new BudgetManagement.Application.BudgetGroups.BudgetGroupDto { Id = id }
                    : null);
        }

        private void SetupSoftDeleteValidation(int id, bool hasChildren = false)
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(hasChildren);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupExists(1);
            SetupSoftDeleteValidation(1, hasChildren: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteBudgetGroupCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            SetupExists(0, exists: false);
            SetupSoftDeleteValidation(0, hasChildren: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteBudgetGroupCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("Id is required.");
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupExists(99, exists: false);
            SetupSoftDeleteValidation(99, hasChildren: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteBudgetGroupCommand { Id = 99 });

            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("BudgetGroup not found.");
        }

        [Fact]
        public async Task Validate_HasChildren_FailsValidation()
        {
            SetupExists(1);
            SetupSoftDeleteValidation(1, hasChildren: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteBudgetGroupCommand { Id = 1 });

            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("Cannot delete BudgetGroup as it has child BudgetGroups.");
        }
    }
}
