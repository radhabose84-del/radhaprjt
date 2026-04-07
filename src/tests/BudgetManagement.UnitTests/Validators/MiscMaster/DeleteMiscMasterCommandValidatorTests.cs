using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using BudgetManagement.Presentation.Validation.MiscMaster;
using FluentValidation.TestHelper;

namespace BudgetManagement.UnitTests.Validators.MiscMaster
{
    public sealed class DeleteMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupNotFound(int id, bool notFound)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(notFound);
        }

        private void SetupSoftDeleteValidation(int id, bool isLinked)
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(id))
                .ReturnsAsync(isLinked);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupNotFound(1, notFound: false);
            SetupSoftDeleteValidation(1, isLinked: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            SetupNotFound(0, notFound: true);
            SetupSoftDeleteValidation(0, isLinked: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupNotFound(99, notFound: true);
            SetupSoftDeleteValidation(99, isLinked: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_LinkedWithOtherRecords_FailsValidation()
        {
            SetupNotFound(1, notFound: false);
            SetupSoftDeleteValidation(1, isLinked: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand { Id = 1 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
