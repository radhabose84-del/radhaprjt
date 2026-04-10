using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.CreateAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Presentation.Validation.AssetMaster.AssetMasterGeneral;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetMasterGeneral
{
    public sealed class CreateAssetMasterGeneralCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateAssetMasterGeneralCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        private static CreateAssetMasterGeneralCommand ValidCommand() =>
            new CreateAssetMasterGeneralCommand
            {
                AssetMaster = new AssetMasterDto
                {
                    AssetName = "Test Asset",
                    CompanyId = 1,
                    UnitId = 1,
                    AssetGroupId = 1,
                    AssetCategoryId = 1,
                    AssetSubCategoryId = 1,
                    Quantity = 1,
                    UOMId = 1,
                    AssetLocation = new AssetLocationCombineDto
                    {
                        UnitId = 1,
                        DepartmentId = 1,
                        LocationId = 1,
                        SubLocationId = 1
                    }
                }
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyAssetName_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetMaster!.AssetName = "";

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroCompanyId_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetMaster!.CompanyId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroQuantity_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetMaster!.Quantity = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
