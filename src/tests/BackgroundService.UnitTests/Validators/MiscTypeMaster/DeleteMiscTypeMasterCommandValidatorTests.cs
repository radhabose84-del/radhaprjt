using BackgroundService.Application.Common.Interfaces.IMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BackgroundService.Presentation.Validation.MiscTypeMaster;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.MiscTypeMaster
{
    public sealed class DeleteMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true); // true = found

            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(id))
                .ReturnsAsync(false); // false = no linked records, safe to delete
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteMiscTypeMasterCommand { Id = 1 };
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteMiscTypeMasterCommand { Id = 0 };
            // Async rules still run for Id=0
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(0))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new DeleteMiscTypeMasterCommand { Id = 999 };
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(999))
                .ReturnsAsync(false); // false = not found

            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(999))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_HasLinkedRecords_FailsValidation()
        {
            var command = new DeleteMiscTypeMasterCommand { Id = 1 };
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(true); // found

            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(true); // true = has linked records

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingIdNoLinkedRecords_PassesValidation()
        {
            var command = new DeleteMiscTypeMasterCommand { Id = 5 };
            SetupAllAsyncMocks(5);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
