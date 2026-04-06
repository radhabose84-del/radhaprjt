using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using MaintenanceManagement.Presentation.Validation.MiscMaster;

namespace MaintenanceManagement.UnitTests.Validators.MiscMaster
{
    public sealed class DeleteMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupNotFound(int id, bool notFound)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(notFound);
        }

        private void SetupSoftDelete(int id, bool linked)
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(linked);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupSoftDelete(1, linked: false);
            SetupNotFound(1, notFound: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_Linked_FailsValidation()
        {
            SetupSoftDelete(1, linked: true);
            SetupNotFound(1, notFound: false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand { Id = 1 });

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupSoftDelete(99, linked: false);
            SetupNotFound(99, notFound: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
