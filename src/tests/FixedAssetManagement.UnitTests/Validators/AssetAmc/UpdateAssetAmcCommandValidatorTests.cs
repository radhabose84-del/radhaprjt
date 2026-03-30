using FAM.Application.AssetMaster.AssetAmc.Command.UpdateAssetAmc;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Presentation.Validation.AssetMaster.AssetAmc;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetAmc
{
    public sealed class UpdateAssetAmcCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IAssetAmcQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateAssetAmcCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int assetId = 1, int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.ActiveAMCValidation(assetId, (int?)id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetAmcBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks(command.AssetId, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyVendorCode_FailsValidation(string? vendorCode)
        {
            var command = AssetAmcBuilders.ValidUpdateCommand(vendorCode: vendorCode!);
            _mockQueryRepo
                .Setup(r => r.ActiveAMCValidation(It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyVendorName_FailsValidation(string? vendorName)
        {
            var command = AssetAmcBuilders.ValidUpdateCommand(vendorName: vendorName!);
            _mockQueryRepo
                .Setup(r => r.ActiveAMCValidation(It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
