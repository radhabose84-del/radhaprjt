using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FluentValidation.TestHelper;
using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.Quotation.QuotationCompare;

namespace PurchaseManagement.UnitTests.Validators.Quotation.QuotationCompare
{
    public sealed class CreateQuotationCompareValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private CreateQuotationCompareValidator CreateValidator()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockWorkflowLookup
                .Setup(w => w.IsApproveWorkflowConfigureAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            return new CreateQuotationCompareValidator(
                _mockMaxLength.Object, _mockWorkflowLookup.Object, _mockIpService.Object);
        }

        private static CreateQuoteComparsionCommand ValidCommand(int rfqId = 1, string rfqCode = "RFQ001") =>
            new()
            {
                CreateQuoteComparsion = new CreateQuoteComparsionDto
                {
                    RfqId = rfqId,
                    RfqCode = rfqCode,
                    Details = new List<CreateQuoteComparsionDto.CreateQuoteComparsionDetailDto>
                    {
                        new()
                        {
                            QuotationHeaderId = 1,
                            QuotationDetailId = 1,
                            Net = 100,
                            LandedUnit = 110,
                            Total = 1100
                        }
                    }
                }
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveValidationErrorFor("CreateQuoteComparsion.RfqId");
            result.ShouldNotHaveValidationErrorFor("CreateQuoteComparsion.RfqCode");
        }

        [Fact]
        public async Task Validate_EmptyRfqId_FailsValidation()
        {
            var command = new CreateQuoteComparsionCommand
            {
                CreateQuoteComparsion = new CreateQuoteComparsionDto
                {
                    RfqId = 0,
                    RfqCode = "RFQ001",
                    Details = new List<CreateQuoteComparsionDto.CreateQuoteComparsionDetailDto>()
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e => e.PropertyName.Contains("RfqId"));
        }

        // The validator has no uniqueness rule: a valid command must never
        // raise an "already exists" error, so re-submitting is not blocked.
        [Fact]
        public async Task Validate_ExistingComparison_DoesNotRaiseDuplicateError()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.Errors.Should().NotContain(e => e.ErrorMessage.Contains("already exists"));
        }
    }
}
