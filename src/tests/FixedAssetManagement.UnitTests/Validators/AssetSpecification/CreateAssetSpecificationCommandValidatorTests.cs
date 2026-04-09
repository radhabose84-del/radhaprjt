using FAM.Application.AssetMaster.AssetSpecification.Commands.CreateAssetSpecification;
using FAM.Presentation.Validation.AssetMaster.AssetSpecification;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetSpecification
{
    public sealed class CreateAssetSpecificationCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateAssetSpecificationCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetSpecificationBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroAssetId_FailsValidation()
        {
            var command = AssetSpecificationBuilders.ValidCreateCommand(assetId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptySpecifications_FailsValidation()
        {
            var command = new CreateAssetSpecificationCommand
            {
                AssetId = 1,
                Specifications = new List<SpecificationItem>()
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptySpecificationValue_FailsValidation()
        {
            var command = new CreateAssetSpecificationCommand
            {
                AssetId = 1,
                Specifications = new List<SpecificationItem>
                {
                    new SpecificationItem { SpecificationId = 1, SpecificationValue = "" }
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
