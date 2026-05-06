using FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Presentation.Validation.AssetMaster.AssetTransferIssue;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetTransferIssue
{
    public sealed class CreateAssetTransferIssueCommandValidatorTests
    {
        private readonly Mock<IAssetTransferQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateAssetTransferIssueCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

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

        // SCRUM-1463 — duplicate-asset guard

        [Fact]
        public async Task Validate_AssetAlreadyPending_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.IsAssetPendingOrApprovedAsync(1, null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldHaveValidationErrorFor(x => x.AssetTransferIssueHdrDto!.AssetTransferIssueDtls)
                  .WithErrorMessage("This asset already has a pending transfer awaiting approval.");
        }

        [Fact]
        public async Task Validate_NoneOfTheAssetsArePending_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.IsAssetPendingOrApprovedAsync(It.IsAny<int>(), null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.Errors.Should().NotContain(e =>
                e.ErrorMessage.StartsWith("This asset already has a pending transfer"));
        }

        [Fact]
        public async Task Validate_SameAssetListedTwice_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetTransferIssueHdrDto!.AssetTransferIssueDtls = new List<AssetTransferIssueDtlDto>
            {
                new AssetTransferIssueDtlDto { AssetId = 5, AssetValue = 1000m },
                new AssetTransferIssueDtlDto { AssetId = 5, AssetValue = 2000m }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AssetTransferIssueHdrDto!.AssetTransferIssueDtls)
                  .WithErrorMessage("The same asset cannot appear more than once in the transfer.");
        }
    }
}
