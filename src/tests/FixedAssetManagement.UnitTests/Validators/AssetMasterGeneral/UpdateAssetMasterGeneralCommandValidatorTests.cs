using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UpdateAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Presentation.Validation.AssetMaster.AssetMasterGeneral;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetMasterGeneral
{
    public sealed class UpdateAssetMasterGeneralCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateAssetMasterGeneralCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        private static UpdateAssetMasterGeneralCommand ValidCommand() =>
            new UpdateAssetMasterGeneralCommand
            {
                AssetMaster = new AssetMasterUpdateDto
                {
                    Id = 1,
                    AssetName = "Test Asset",
                    CompanyId = 1,
                    UnitId = 1,
                    AssetGroupId = 1,
                    AssetCategoryId = 1,
                    AssetSubCategoryId = 1,
                    AssetType = 1,
                    Quantity = 1,
                    UOMId = 1,
                    WorkingStatus = 1,
                    AssetLocation = new AssetLocationUpdateDto
                    {
                        UnitId = 1,
                        DepartmentId = 1,
                        LocationId = 1,
                        SubLocationId = 1
                    },
                    AssetPurchaseDetails = new List<AssetPurchaseUpdateDto>
                    {
                        new AssetPurchaseUpdateDto
                        {
                            VendorCode = "V001",
                            VendorName = "Vendor",
                            PoNo = 1,
                            PoSno = 1,
                            ItemCode = "ITM001",
                            ItemName = "Item",
                            GrnNo = 1,
                            GrnSno = 1,
                            AcceptedQty = 1,
                            PurchaseValue = 1000m,
                            GrnValue = 1000m,
                            BillNo = "B001",
                            PjYear = "2025",
                            PjDocId = "DOC1",
                            PjDocNo = 1
                        }
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
    }
}
