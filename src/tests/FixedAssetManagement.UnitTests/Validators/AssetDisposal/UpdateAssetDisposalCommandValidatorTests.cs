using FAM.Application.AssetMaster.AssetDisposal.Command.UpdateAssetDisposal;
using FAM.Presentation.Validation.AssetMaster.AssetDisposal;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetDisposal
{
    public sealed class UpdateAssetDisposalCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateAssetDisposalCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetDisposalBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NullDisposalAmount_FailsValidation()
        {
            var command = AssetDisposalBuilders.ValidUpdateCommand();
            command.DisposalAmount = null;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NullDisposalType_FailsValidation()
        {
            var command = AssetDisposalBuilders.ValidUpdateCommand();
            command.DisposalType = null;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
