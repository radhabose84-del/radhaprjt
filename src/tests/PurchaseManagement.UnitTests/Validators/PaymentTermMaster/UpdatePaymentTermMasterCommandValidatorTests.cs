using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Presentation.Validation.PaymentTermMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PaymentTermMaster
{
    public sealed class UpdatePaymentTermMasterCommandValidatorTests
    {
        private readonly Mock<IPaymentTermMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdatePaymentTermMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupCodeNotExists(string code = "PT001", int excludeId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.ExistsByCodeAsync(code.Trim(), excludeId))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = PaymentTermMasterBuilders.ValidUpdateCommand(1, "PT001");
            SetupCodeNotExists("PT001", 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = PaymentTermMasterBuilders.ValidUpdateCommand(0, "PT001");
            SetupCodeNotExists("PT001", 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = PaymentTermMasterBuilders.ValidUpdateCommand(1, code!);
            // Code is empty so no need to mock ExistsByCodeAsync

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = PaymentTermMasterBuilders.ValidUpdateCommand(1, "DUP001");
            _mockQueryRepo
                .Setup(r => r.ExistsByCodeAsync("DUP001", 1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = PaymentTermMasterBuilders.ValidUpdateCommand(1, "PT001", description!);
            SetupCodeNotExists("PT001", 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }
}
