using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.SubmitFreightRfqForApproval;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Presentation.Validation.FreightRfq;

namespace PurchaseManagement.UnitTests.Validators.FreightRfq
{
    public sealed class SubmitFreightRfqForApprovalCommandValidatorTests
    {
        private readonly Mock<IFreightRfqQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private SubmitFreightRfqForApprovalCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetStatusCodeAsync(It.IsAny<int>())).ReturnsAsync(MiscEnumEntity.FreightRfqQuotationPending);
            _mockQueryRepo.Setup(r => r.GetQuotationCountAsync(It.IsAny<int>())).ReturnsAsync(2);
            _mockQueryRepo.Setup(r => r.QuotationBelongsToRfqAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
        }

        private static SubmitFreightRfqForApprovalCommand ValidCommand(bool isOverride = false, string? remarks = null) =>
            new() { FreightRfqId = 1, SelectedQuotationId = 2, IsOverride = isOverride, ComparisonRemarks = remarks };

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NoSelectedTransporter_Fails()
        {
            SetupHappyPath();
            var noSelection = new SubmitFreightRfqForApprovalCommand { FreightRfqId = 1, SelectedQuotationId = 0 };
            var result = await CreateValidator().TestValidateAsync(noSelection);
            result.ShouldHaveValidationErrorFor(x => x.SelectedQuotationId);
        }

        [Fact]
        public async Task Validate_NotDraft_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.GetStatusCodeAsync(It.IsAny<int>())).ReturnsAsync(MiscEnumEntity.Pending);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.FreightRfqId);
        }

        [Fact]
        public async Task Validate_NoQuotations_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.GetQuotationCountAsync(It.IsAny<int>())).ReturnsAsync(0);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.FreightRfqId);
        }

        [Fact]
        public async Task Validate_OverrideWithoutRemarks_Fails()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand(isOverride: true, remarks: null));
            result.ShouldHaveValidationErrorFor(x => x.ComparisonRemarks);
        }

        [Fact]
        public async Task Validate_OverrideWithRemarks_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand(isOverride: true, remarks: "Reliable carrier"));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
