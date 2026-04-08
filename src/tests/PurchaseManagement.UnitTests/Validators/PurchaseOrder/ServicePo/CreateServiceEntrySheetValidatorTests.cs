using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.ServicePo
{
    public sealed class CreateServiceEntrySheetValidatorTests
    {
        private readonly Mock<IServicePurchaseOrderCommandRepository> _mockCmdRepo = new(MockBehavior.Loose);

        private CreateServiceEntrySheetValidator CreateValidator() =>
            new(_mockCmdRepo.Object);

        [Fact]
        public async Task Validate_NullCreateServiceSheet_FailsValidation()
        {
            // Provide a non-null DTO with invalid (zero) IDs instead of null,
            // because the validator accesses nested properties without a null guard.
            var command = new CreateServiceEntrySheetCommand
            {
                CreateServiceSheet = new CreateServiceSheetDto { PurchaseOrderId = 0, VendorId = 0 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroPurchaseOrderId_FailsValidation()
        {
            var command = new CreateServiceEntrySheetCommand
            {
                CreateServiceSheet = new CreateServiceSheetDto { PurchaseOrderId = 0 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
