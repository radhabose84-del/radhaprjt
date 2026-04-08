using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.CreateAssetAdditionalCost;
using FAM.Presentation.Validation.AssetMaster.AssetAdditionalCost;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetAdditionalCost
{
    public sealed class CreateAssetAdditionalCostCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateAssetAdditionalCostCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetAdditionalCostBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroAssetId_FailsValidation()
        {
            var command = AssetAdditionalCostBuilders.ValidCreateCommand(assetId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroAmount_FailsValidation()
        {
            var command = AssetAdditionalCostBuilders.ValidCreateCommand(amount: 0m);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyJournalNo_FailsValidation()
        {
            var command = AssetAdditionalCostBuilders.ValidCreateCommand();
            command.JournalNo = "";

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
