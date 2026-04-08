using FAM.Application.AssetMaster.AssetSpecification.Commands.DeleteAssetSpecification;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Presentation.Validation.AssetMaster.AssetSpecification;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetSpecification
{
    public sealed class DeleteAssetSpecificationCommandValidatorTests
    {
        private readonly Mock<IAssetSpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteAssetSpecificationCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteAssetSpecificationCommand { Id = 1 };
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteAssetSpecificationCommand { Id = 0 };
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_LinkedRecords_FailsValidation()
        {
            var command = new DeleteAssetSpecificationCommand { Id = 1 };
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
