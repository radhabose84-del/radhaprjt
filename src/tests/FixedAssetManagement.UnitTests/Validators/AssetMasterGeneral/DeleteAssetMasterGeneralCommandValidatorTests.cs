using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Presentation.Validation.AssetMaster.AssetMasterGeneral;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetMasterGeneral
{
    public sealed class DeleteAssetMasterGeneralCommandValidatorTests
    {
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteAssetMasterGeneralCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.GetAssetChildDetails(id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteAssetMasterGeneralCommand { Id = 1 };
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteAssetMasterGeneralCommand { Id = 0 };
            _mockQueryRepo
                .Setup(r => r.GetAssetChildDetails(0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_LinkedRecords_FailsValidation()
        {
            var command = new DeleteAssetMasterGeneralCommand { Id = 1 };
            _mockQueryRepo
                .Setup(r => r.GetAssetChildDetails(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
