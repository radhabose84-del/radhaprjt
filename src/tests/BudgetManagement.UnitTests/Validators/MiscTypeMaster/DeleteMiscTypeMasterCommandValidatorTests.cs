using BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BudgetManagement.Presentation.Validation.MiscTypeMaster;
using FluentValidation.TestHelper;

namespace BudgetManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class DeleteMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupNotFound(int id, bool notFound)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(notFound);
        }

        private void SetupSoftDeleteValidation(int id, bool hasChildren)
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(id))
                .ReturnsAsync(hasChildren);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupNotFound(1, notFound: false);
            SetupSoftDeleteValidation(1, hasChildren: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            SetupNotFound(0, notFound: true);
            SetupSoftDeleteValidation(0, hasChildren: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupNotFound(99, notFound: true);
            SetupSoftDeleteValidation(99, hasChildren: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_HasChildren_FailsValidation()
        {
            SetupNotFound(1, notFound: false);
            SetupSoftDeleteValidation(1, hasChildren: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand { Id = 1 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
