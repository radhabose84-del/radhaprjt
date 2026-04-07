using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.DeletePaymentTermMaster;
using PurchaseManagement.Presentation.Validation.PaymentTermMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PaymentTermMaster
{
    public sealed class DeletePaymentTermMasterCommandValidatorTests
    {
        private readonly Mock<IPaymentTermMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeletePaymentTermMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            var command = PaymentTermMasterBuilders.ValidDeleteCommand(1);
            _mockQueryRepo.Setup(r => r.ExistsByIdAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeletePaymentTermMasterCommand { Id = 0 };
            _mockQueryRepo.Setup(r => r.ExistsByIdAsync(0)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = PaymentTermMasterBuilders.ValidDeleteCommand(99);
            _mockQueryRepo.Setup(r => r.ExistsByIdAsync(99)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_LinkedWithOtherRecords_FailsValidation()
        {
            var command = PaymentTermMasterBuilders.ValidDeleteCommand(1);
            _mockQueryRepo.Setup(r => r.ExistsByIdAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
