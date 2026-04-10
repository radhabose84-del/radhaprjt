using FAM.Application.AssetMaster.AssetSpecification.Commands.UpdateAssetSpecification;
using FAM.Presentation.Validation.AssetMaster.AssetSpecification;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetSpecification
{
    public sealed class UpdateAssetSpecificationCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateAssetSpecificationCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateAssetSpecificationCommand
            {
                AssetId = 1,
                Specifications = new List<UpdateSpecificationItem>
                {
                    new UpdateSpecificationItem { SpecificationId = 1, SpecificationValue = "100kg", IsActive = 1 }
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroAssetId_FailsValidation()
        {
            var command = new UpdateAssetSpecificationCommand
            {
                AssetId = 0,
                Specifications = new List<UpdateSpecificationItem>
                {
                    new UpdateSpecificationItem { SpecificationId = 1, SpecificationValue = "100kg", IsActive = 1 }
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptySpecifications_FailsValidation()
        {
            var command = new UpdateAssetSpecificationCommand
            {
                AssetId = 1,
                Specifications = new List<UpdateSpecificationItem>()
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
