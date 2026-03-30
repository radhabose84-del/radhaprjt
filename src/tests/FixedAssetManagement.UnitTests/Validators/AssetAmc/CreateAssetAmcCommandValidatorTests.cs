using FAM.Application.AssetMaster.AssetAmc.Command.CreateAssetAmc;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Presentation.Validation.AssetMaster.AssetAmc;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetAmc
{
    public sealed class CreateAssetAmcCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IAssetAmcQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateAssetAmcCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int assetId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.ActiveAMCValidation(assetId, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetAmcBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.AssetId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroAssetId_FailsValidation()
        {
            var command = AssetAmcBuilders.ValidCreateCommand(assetId: 0);
            _mockQueryRepo
                .Setup(r => r.ActiveAMCValidation(0, null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyVendorCode_FailsValidation(string? vendorCode)
        {
            var command = AssetAmcBuilders.ValidCreateCommand(vendorCode: vendorCode!);
            _mockQueryRepo
                .Setup(r => r.ActiveAMCValidation(It.IsAny<int>(), null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyVendorName_FailsValidation(string? vendorName)
        {
            var command = AssetAmcBuilders.ValidCreateCommand(vendorName: vendorName!);
            _mockQueryRepo
                .Setup(r => r.ActiveAMCValidation(It.IsAny<int>(), null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
