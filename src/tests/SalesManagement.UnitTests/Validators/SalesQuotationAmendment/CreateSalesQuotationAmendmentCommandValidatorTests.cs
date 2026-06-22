using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Presentation.Validation.SalesQuotationAmendment;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesQuotationAmendment
{
    public sealed class CreateSalesQuotationAmendmentCommandValidatorTests
    {
        private readonly Mock<ISalesQuotationAmendmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IWorkflowLookup> _mockWorkflow = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateSalesQuotationAmendmentCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockWorkflow.Object, _mockIp.Object);

        private static CreateSalesQuotationAmendmentCommand ValidCommand() => new()
        {
            SalesQuotationHeaderId = 1,
            Reason = "Price revision",
            FreightCharges = 0m,
            OtherCharges = 0m,
            AmendmentDetails = new List<CreateSalesQuotationAmendmentDetailDto>
            {
                new() { SalesQuotationDetailId = 10, NewQuantity = 5m }
            }
        };

        private void SetupValid()
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockWorkflow
                .Setup(w => w.IsApproveWorkflowConfigureAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesQuotationExistsAndApprovedAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.HasPendingAmendmentAsync(1)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupValid();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SalesQuotationHeaderId_ZeroOrNegative_FailsValidation(int id)
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.SalesQuotationHeaderId = id;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.SalesQuotationHeaderId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Reason_Empty_FailsValidation(string? reason)
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.Reason = reason;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Reason);
        }

        [Fact]
        public async Task NotApprovedQuotation_FailsValidation()
        {
            SetupValid();
            _mockQueryRepo.Setup(r => r.SalesQuotationExistsAndApprovedAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.SalesQuotationHeaderId);
        }

        [Fact]
        public async Task PendingAmendmentExists_FailsValidation()
        {
            SetupValid();
            _mockQueryRepo.Setup(r => r.HasPendingAmendmentAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.SalesQuotationHeaderId);
        }
    }
}
