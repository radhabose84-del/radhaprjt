using FAM.Application.AssetMaster.AssetTranferIssueApproval.Commands.UpdateAssetTranferIssueApproval;
using FAM.Presentation.Validation.AssetMaster.AssetTransferIssueApproval;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetTransferIssueApproval
{
    public sealed class UpdateAssetTransferIssueApprovalValidatorTests
    {
        private UpdateAssetTransferIssueApprovalValidator CreateValidator() => new();

        private static UpdateAssetTranferIssueApprovalCommand ValidCommand(string status = "Approved") =>
            new UpdateAssetTranferIssueApprovalCommand(new List<int> { 1, 2 }, status);

        [Fact]
        public async Task Validate_Approved_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand("Approved"));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_Rejected_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand("Rejected"));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("Pending")]
        [InlineData("approved")]
        [InlineData("REJECTED")]
        [InlineData("Unknown")]
        public async Task Validate_InvalidStatus_FailsValidation(string status)
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand(status));

            result.ShouldHaveValidationErrorFor(x => x.Status);
        }

        [Fact]
        public async Task Validate_EmptyStatus_ReturnsStatusIsRequiredError()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand(string.Empty));

            result.ShouldHaveValidationErrorFor(x => x.Status)
                  .WithErrorMessage("Status is required.");
        }

        [Fact]
        public async Task Validate_NonApprovedOrRejected_ReturnsInvalidStatusError()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand("Hold"));

            result.ShouldHaveValidationErrorFor(x => x.Status)
                  .WithErrorMessage("Invalid status Status Should be Approved or Rejected.");
        }
    }
}
