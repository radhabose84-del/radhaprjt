using FAM.Application.SpecificationMaster.Commands.CreateSpecificationMaster;
using FAM.Presentation.Validation.Common;
using FAM.Presentation.Validation.SpecificationMaster;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.SpecificationMasters
{
    public sealed class CreateSpecificationMasterCommandValidatorTests
    {
        private CreateSpecificationMasterCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!));

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = SpecificationMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptySpecificationName_FailsValidation(string? specificationName)
        {
            var command = SpecificationMasterBuilders.ValidCreateCommand(specificationName: specificationName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroAssetGroupId_FailsValidation()
        {
            var command = SpecificationMasterBuilders.ValidCreateCommand(assetGroupId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NegativeAssetGroupId_FailsValidation()
        {
            var command = SpecificationMasterBuilders.ValidCreateCommand(assetGroupId: -1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
