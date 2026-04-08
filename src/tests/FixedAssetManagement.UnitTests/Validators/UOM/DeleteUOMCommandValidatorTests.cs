using FAM.Application.Common.Interfaces.IUOM;
using FAM.Application.UOM.Command.DeleteUOM;
using FAM.Presentation.Validation.UOM;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.UOM
{
    public sealed class DeleteUOMCommandValidatorTests
    {
        private readonly Mock<IUOMQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteUOMCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteUOMCommand { Id = 1 };
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteUOMCommand { Id = 0 };
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_LinkedRecords_FailsValidation()
        {
            var command = new DeleteUOMCommand { Id = 1 };
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
