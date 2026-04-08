using FAM.Application.AssetMaster.AssetTransferReceipt.Command.CreateAssetTransferReceipt;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptPending;
using FAM.Presentation.Validation.AssetMaster.AssetTransferReceipt;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetTransferReceipt
{
    public sealed class CreateAssetTransferReceiptCommandValidatorTests
    {
        private CreateAssetTransferReceiptCommandValidator CreateValidator() => new();

        private static CreateAssetTransferReceiptCommand ValidCommand() =>
            new CreateAssetTransferReceiptCommand
            {
                AssetTransferReceiptHdrDto = new AssetTransferReceiptHdrDto
                {
                    DocDate = DateTimeOffset.UtcNow.AddMinutes(-5),
                    AssetTransferId = 1,
                    AssetTransferReceiptDtl = new List<AssetTransferReceiptDtlDto>
                    {
                        new AssetTransferReceiptDtlDto { AssetId = 1 }
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
        public async Task Validate_FutureDocDate_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetTransferReceiptHdrDto!.DocDate = DateTimeOffset.UtcNow.AddDays(10);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroAssetId_InDetail_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetTransferReceiptHdrDto!.AssetTransferReceiptDtl = new List<AssetTransferReceiptDtlDto>
            {
                new AssetTransferReceiptDtlDto { AssetId = 0 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
