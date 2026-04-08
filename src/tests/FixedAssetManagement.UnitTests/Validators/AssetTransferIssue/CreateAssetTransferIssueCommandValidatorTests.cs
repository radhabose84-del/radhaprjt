using FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Presentation.Validation.AssetMaster.AssetTransferIssue;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetTransferIssue
{
    public sealed class CreateAssetTransferIssueCommandValidatorTests
    {
        private CreateAssetTransferIssueCommandValidator CreateValidator() => new();

        private static CreateAssetTransferIssueCommand ValidCommand() =>
            new CreateAssetTransferIssueCommand
            {
                AssetTransferIssueHdrDto = new AssetTransferIssueHdrDto
                {
                    DocDate = DateTimeOffset.UtcNow.AddMinutes(-5),
                    TransferType = 1,
                    FromUnitId = 1,
                    ToUnitId = 2,
                    FromDepartmentId = 1,
                    ToDepartmentId = 2,
                    FromCustodianId = 1,
                    ToCustodianId = 2,
                    AssetTransferIssueDtls = new List<AssetTransferIssueDtlDto>
                    {
                        new AssetTransferIssueDtlDto { AssetId = 1, AssetValue = 5000m }
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
            command.AssetTransferIssueHdrDto!.FromUnitId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroToUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetTransferIssueHdrDto!.ToUnitId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyDetails_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetTransferIssueHdrDto!.AssetTransferIssueDtls = new List<AssetTransferIssueDtlDto>();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
