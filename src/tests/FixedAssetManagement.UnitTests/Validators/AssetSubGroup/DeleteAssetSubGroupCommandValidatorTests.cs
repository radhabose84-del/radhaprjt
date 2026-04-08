using FAM.Application.AssetSubGroup.Command.DeleteAssetSubGroup;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Presentation.Validation.AssetSubGroup;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetSubGroup
{
    public sealed class DeleteAssetSubGroupCommandValidatorTests
    {
        private readonly Mock<IAssetSubGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteAssetSubGroupCommandValidator CreateValidator() =>
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
            var command = new DeleteAssetSubGroupCommand { Id = 1 };
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteAssetSubGroupCommand { Id = 0 };
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_LinkedRecords_FailsValidation()
        {
            var command = new DeleteAssetSubGroupCommand { Id = 1 };
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
