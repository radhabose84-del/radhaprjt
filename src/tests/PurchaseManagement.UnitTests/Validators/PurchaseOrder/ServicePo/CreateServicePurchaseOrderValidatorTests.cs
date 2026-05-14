using FluentValidation.TestHelper;
using Contracts.Dtos.Validations.MaintenanceManagement;
using Contracts.Interfaces.Lookups.Workflow;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo;
using Shared.Validation.Common;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.ServicePo
{
    public sealed class CreateServicePurchaseOrderValidatorTests
    {
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IServicePurchaseOrderCommandRepository> _mockCmdRepo = new(MockBehavior.Loose);
        private readonly Mock<IMaintenanceRequestValidation> _mockMrValidation = new(MockBehavior.Loose);

        private CreateServicePurchaseOrderValidator CreateValidator()
        {
            var rules = ValidationRuleLoader.LoadValidationRules();
            return new(_mockWorkflowLookup.Object, rules, _mockCmdRepo.Object, _mockMrValidation.Object);
        }

        private static CreateServicePurchaseOrderDto BuildValidDtoWithLinkedRequest(
            int vendorId = 7, int requestId = 501)
        {
            return new CreateServicePurchaseOrderDto
            {
                UnitId = 1,
                PODate = DateTime.UtcNow,
                VendorId = vendorId,
                ServicePos = new List<PurchaseOrderServiceHeaderDto>
                {
                    new()
                    {
                        ServiceCategoryId = 12,
                        Lines = new List<PurchaseOrderServiceLineDto>
                        {
                            new()
                            {
                                LineNo = 1,
                                RequestId = requestId,
                                PlannedQuantity = 1,
                                PlannedRate = 50000,
                                PlannedValue = 50000
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        public async Task Validate_EmptyDto_FailsValidation()
        {
            var dto = new CreateServicePurchaseOrderDto();

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }

        // Rule 1 — eligibility

        [Fact]
        public async Task Validate_EligibleLinkedRequest_PassesEsrRules()
        {
            var dto = BuildValidDtoWithLinkedRequest(vendorId: 7, requestId: 501);
            _mockMrValidation
                .Setup(v => v.IsAvailableForServicePoAsync(501, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockMrValidation
                .Setup(v => v.GetRefAsync(501, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MaintenanceRequestRefDto { Id = 501, VendorId = 7 });

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotContain(e => e.ErrorMessage.Contains("not available"));
        }

        [Fact]
        public async Task Validate_IneligibleLinkedRequest_FailsRule1()
        {
            var dto = BuildValidDtoWithLinkedRequest(vendorId: 7, requestId: 999);
            _mockMrValidation
                .Setup(v => v.IsAvailableForServicePoAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should()
                .Contain(e => e.ErrorMessage.Contains("999")
                           && e.ErrorMessage.Contains("not available"));
        }

        // Rule 2 — vendor consistency

        [Fact]
        public async Task Validate_LinkedRequestVendorMismatch_FailsRule2()
        {
            var dto = BuildValidDtoWithLinkedRequest(vendorId: 7, requestId: 501);
            _mockMrValidation
                .Setup(v => v.IsAvailableForServicePoAsync(501, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockMrValidation
                .Setup(v => v.GetRefAsync(501, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MaintenanceRequestRefDto { Id = 501, VendorId = 999 });

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should()
                .Contain(e => e.ErrorMessage.Contains("501")
                           && e.ErrorMessage.Contains("vendor 999"));
        }

        // Rule 3 — duplicate detection

        [Fact]
        public async Task Validate_SameRequestIdOnTwoLines_FailsRule3()
        {
            var dto = BuildValidDtoWithLinkedRequest(vendorId: 7, requestId: 501);
            // add a second line with the same RequestId
            dto.ServicePos[0].Lines!.Add(new PurchaseOrderServiceLineDto
            {
                LineNo = 2,
                RequestId = 501,
                PlannedQuantity = 1,
                PlannedRate = 10000,
                PlannedValue = 10000
            });

            _mockMrValidation
                .Setup(v => v.IsAvailableForServicePoAsync(501, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockMrValidation
                .Setup(v => v.GetRefAsync(501, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MaintenanceRequestRefDto { Id = 501, VendorId = 7 });

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should()
                .Contain(e => e.ErrorMessage.Contains("501")
                           && e.ErrorMessage.Contains("more than one line"));
        }

        // No linkage — bypass

        [Fact]
        public async Task Validate_NoLinkedRequests_DoesNotCallEsrValidation()
        {
            var dto = new CreateServicePurchaseOrderDto
            {
                UnitId = 1,
                PODate = DateTime.UtcNow,
                VendorId = 7,
                ServicePos = new List<PurchaseOrderServiceHeaderDto>
                {
                    new()
                    {
                        ServiceCategoryId = 12,
                        Lines = new List<PurchaseOrderServiceLineDto>
                        {
                            new() { LineNo = 1, RequestId = null, PlannedQuantity = 1, PlannedRate = 10, PlannedValue = 10 }
                        }
                    }
                }
            };

            await CreateValidator().TestValidateAsync(dto);

            _mockMrValidation.Verify(
                v => v.IsAvailableForServicePoAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Validate_RequestIdZero_TreatedAsNoLink()
        {
            // Legacy data: frontends may still send 0; validator must treat as "no link"
            var dto = BuildValidDtoWithLinkedRequest(vendorId: 7, requestId: 0);

            await CreateValidator().TestValidateAsync(dto);

            _mockMrValidation.Verify(
                v => v.IsAvailableForServicePoAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
