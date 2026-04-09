using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.Quotation.QuotationCompare;

namespace PurchaseManagement.UnitTests.Validators.Quotation.QuotationCompare
{
    public sealed class CreateQuotationCompareValidatorTests
    {
        private readonly Mock<IQuotationCompareCommandRepository> _mockCmdRepo = new(MockBehavior.Loose);
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
                _mockCmdRepo.Object, _mockMaxLength.Object,
                _mockWorkflowLookup.Object, _mockIpService.Object);
        }

        private void SetupAllAsyncMocks()
        {
            _mockCmdRepo
                .Setup(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = new CreateQuoteComparsionCommand
            {
                CreateQuoteComparsion = new CreateQuoteComparsionDto
                {
                    RfqId = 1,
                    RfqCode = "RFQ001",
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

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor("CreateQuoteComparsion.RfqId");
            result.ShouldNotHaveValidationErrorFor("CreateQuoteComparsion.RfqCode");
        }

        [Fact]
        public async Task Validate_EmptyRfqId_FailsValidation()
        {
            SetupAllAsyncMocks();
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

            result.Errors.Should().Contain(e =>
                e.PropertyName.Contains("RfqId"));
        }

        [Fact]
        public async Task Validate_DuplicateComparison_FailsValidation()
        {
            _mockCmdRepo
                .Setup(r => r.ExistsAsync(1, "RFQ001"))
                .ReturnsAsync(true);
            var command = new CreateQuoteComparsionCommand
            {
                CreateQuoteComparsion = new CreateQuoteComparsionDto
                {
                    RfqId = 1,
                    RfqCode = "RFQ001",
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

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("already exists"));
        }
    }
}
