using FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue;
using FAM.Presentation.Validation.AssetMaster.AssetTransferIssue;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetTransferIssue
{
    public sealed class UpdateAssetTransferIssueCommandValidatorTests
    {
        private UpdateAssetTransferIssueCommandValidator CreateValidator() => new();

        private static UpdateAssetTransferIssueCommand ValidCommand() =>
            new UpdateAssetTransferIssueCommand
            {
                AssetTransferHdr = new UpdateAssetTransferHdrDto
                {
                    Id = 1,
                    DocDate = DateTime.UtcNow.AddMinutes(-5),
                    TransferType = 1,
                    FromUnitId = 1,
                    ToUnitId = 2,
                    FromDepartmentId = 1,
                    ToDepartmentId = 2,
                    FromCustodianId = 1,
                    ToCustodianId = 2,
                    Status = "Pending",
                    AssetTransferIssueDtl = new List<UpdateAssetTransferDtlDto>
                    {
                        new UpdateAssetTransferDtlDto { AssetTransferId = 1, AssetId = 1, AssetValue = 5000m }
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
        public async Task Validate_ZeroFromUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetTransferHdr!.FromUnitId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroToUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetTransferHdr!.ToUnitId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_InvalidStatus_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetTransferHdr!.Status = "Invalid";

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyDetails_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetTransferHdr!.AssetTransferIssueDtl = new List<UpdateAssetTransferDtlDto>();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroAssetId_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetTransferHdr!.AssetTransferIssueDtl = new List<UpdateAssetTransferDtlDto>
            {
                new UpdateAssetTransferDtlDto { AssetTransferId = 1, AssetId = 0, AssetValue = 5000m }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
