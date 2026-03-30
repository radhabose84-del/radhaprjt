using FAM.Application.AssetMaster.AssetDisposal.Command.CreateAssetDisposal;
using FAM.Presentation.Validation.AssetMaster.AssetDisposal;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetDisposal
{
    public sealed class CreateAssetDisposalCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateAssetDisposalCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetDisposalBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroAssetId_FailsValidation()
        {
            var command = AssetDisposalBuilders.ValidCreateCommand(assetId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroAssetPurchaseId_FailsValidation()
        {
            var command = AssetDisposalBuilders.ValidCreateCommand(assetPurchaseId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NullDisposalAmount_FailsValidation()
        {
            var command = AssetDisposalBuilders.ValidCreateCommand();
            command.DisposalAmount = null;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
