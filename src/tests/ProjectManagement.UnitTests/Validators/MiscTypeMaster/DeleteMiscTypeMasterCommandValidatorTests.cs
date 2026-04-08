using FluentValidation.TestHelper;
using ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using ProjectManagement.Presentation.Validation.MiscTypeMaster;

namespace ProjectManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class DeleteMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks(1);
            var command = new DeleteMiscTypeMasterCommand { Id = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteMiscTypeMasterCommand { Id = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(999))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(999))
                .ReturnsAsync(false);
            var command = new DeleteMiscTypeMasterCommand { Id = 999 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_LinkedRecords_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(1))
                .ReturnsAsync(true);
            var command = new DeleteMiscTypeMasterCommand { Id = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("linked"));
        }
    }
}
