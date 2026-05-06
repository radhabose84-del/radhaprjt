using FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Presentation.Validation.AssetMaster.AssetTransferIssue;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetTransferIssue
{
    public sealed class UpdateAssetTransferIssueCommandValidatorTests
    {
        private readonly Mock<IAssetTransferQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateAssetTransferIssueCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

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

        // SCRUM-1463 — duplicate-asset guard on Update flow

        [Fact]
        public async Task Validate_ReassigningToBusyAsset_FailsValidation()
        {
            // Updating transfer 1 with asset 5 — but asset 5 is already pending under a different transfer.
            // excludeTransferId = 1 (the request's own Id) is passed; repo still returns true → blocked.
            _mockQueryRepo
                .Setup(r => r.IsAssetPendingOrApprovedAsync(5, 1))
                .ReturnsAsync(true);

            var command = ValidCommand();
            command.AssetTransferHdr!.AssetTransferIssueDtl = new List<UpdateAssetTransferDtlDto>
            {
                new UpdateAssetTransferDtlDto { AssetTransferId = 1, AssetId = 5, AssetValue = 1000m }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AssetTransferHdr!.AssetTransferIssueDtl)
                  .WithErrorMessage("This asset already has a pending transfer awaiting approval.");
        }

        [Fact]
        public async Task Validate_SelfUpdate_DoesNotTripDuplicateGuard()
        {
            // Self-update: only the same transfer holds asset 5; with excludeTransferId = 1 the repo
            // returns false because the request's own row is excluded.
            _mockQueryRepo
                .Setup(r => r.IsAssetPendingOrApprovedAsync(5, 1))
                .ReturnsAsync(false);

            var command = ValidCommand();
            command.AssetTransferHdr!.AssetTransferIssueDtl = new List<UpdateAssetTransferDtlDto>
            {
                new UpdateAssetTransferDtlDto { AssetTransferId = 1, AssetId = 5, AssetValue = 1000m }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotContain(e =>
                e.ErrorMessage.StartsWith("This asset already has a pending transfer"));
        }

        [Fact]
        public async Task Validate_SameAssetListedTwice_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetTransferHdr!.AssetTransferIssueDtl = new List<UpdateAssetTransferDtlDto>
            {
                new UpdateAssetTransferDtlDto { AssetTransferId = 1, AssetId = 5, AssetValue = 1000m },
                new UpdateAssetTransferDtlDto { AssetTransferId = 1, AssetId = 5, AssetValue = 2000m }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AssetTransferHdr!.AssetTransferIssueDtl)
                  .WithErrorMessage("The same asset cannot appear more than once in the transfer.");
        }
    }
}
