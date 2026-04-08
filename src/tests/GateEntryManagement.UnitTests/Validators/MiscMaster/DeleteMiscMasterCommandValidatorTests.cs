using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Application.MiscMaster.Commands.DeleteMiscMaster;
using GateEntryManagement.Presentation.Validation.MiscMaster;

namespace GateEntryManagement.UnitTests.Validators.MiscMaster
{
    public sealed class DeleteMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteMiscMasterCommand(1);
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteMiscMasterCommand(0);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new DeleteMiscMasterCommand(999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_LinkedRecords_FailsValidation()
        {
            var command = new DeleteMiscMasterCommand(1);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
