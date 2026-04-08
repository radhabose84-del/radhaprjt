using FAM.Application.AssetMaster.AssetPurchase.Commands.UpdateAssetPurchaseDetails;
using FAM.Presentation.Validation.AssetMaster.AssetPurchase;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetPurchaseDetail
{
    public sealed class UpdateAssetPurchaseDetailCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateAssetPurchaseDetailCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        private static UpdateAssetPurchaseDetailCommand ValidCommand() =>
            new UpdateAssetPurchaseDetailCommand
            {
                Id = 1,
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
        public async Task Validate_ZeroPurchaseValue_FailsValidation()
        {
            var command = ValidCommand();
            command.PurchaseValue = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
