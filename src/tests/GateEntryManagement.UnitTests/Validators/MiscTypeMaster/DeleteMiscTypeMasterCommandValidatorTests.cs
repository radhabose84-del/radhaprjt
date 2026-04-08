using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster;
using GateEntryManagement.Presentation.Validation.MiscTypeMaster;

namespace GateEntryManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class DeleteMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteMiscTypeMasterCommand(1);
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteMiscTypeMasterCommand(0);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new DeleteMiscTypeMasterCommand(999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_LinkedRecords_FailsValidation()
        {
            var command = new DeleteMiscTypeMasterCommand(1);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
