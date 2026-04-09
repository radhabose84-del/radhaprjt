using FAM.Application.AssetMaster.AssetPurchase.Commands.CreateAssetPurchaseDetails;
using FAM.Presentation.Validation.AssetMaster.AssetPurchase;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetPurchaseDetail
{
    public sealed class CreateAssetPurchaseDetailCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateAssetPurchaseDetailCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        private static CreateAssetPurchaseDetailCommand ValidCommand() =>
            new CreateAssetPurchaseDetailCommand
            {
                AssetId = 1,
                AssetSourceId = 1,
                OldUnitId = "U1",
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
                PjDocNo = 1,
                QcCompleted = 'Y'
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyVendorCode_FailsValidation()
        {
            var command = ValidCommand();
            command.VendorCode = "";

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyOldUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.OldUnitId = "";

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroPurchaseValue_FailsValidation()
        {
            var command = ValidCommand();
            command.PurchaseValue = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
