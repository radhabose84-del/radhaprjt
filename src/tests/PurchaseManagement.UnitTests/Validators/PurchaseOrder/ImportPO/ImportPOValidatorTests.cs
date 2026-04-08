using FluentValidation.TestHelper;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Validation;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.ImportPO
{
    public sealed class ImportPOValidatorTests
    {
        [Fact]
        public async Task CreateCommand_NullData_FailsValidation()
        {
            var validator = new CreateImportPOCommandValidator();
            var command = new CreateImportPOCommand { Data = null! };

            var result = await validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Data);
        }

        [Fact]
        public async Task ImportPOCreateDto_ZeroVendorId_FailsValidation()
        {
            var validator = new ImportPOCreateDtoValidator();
            var dto = new ImportPOCreateDto { VendorId = 0 };

            var result = await validator.TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
