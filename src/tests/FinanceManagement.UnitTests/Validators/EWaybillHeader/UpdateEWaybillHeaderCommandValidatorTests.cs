using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.UpdateEWaybillHeader;
using FinanceManagement.Presentation.Validation.EWaybillHeader;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.EWaybillHeader
{
    public sealed class UpdateEWaybillHeaderCommandValidatorTests
    {
        private readonly Mock<IEWaybillHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateEWaybillHeaderCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        private static UpdateEWaybillHeaderCommand ValidCommand() =>
            new()
            {
                Id = 1,
                TotalValue = 1000m,
                CGST = 90m,
                SGST = 90m,
                IGST = 0m,
                Cess = 0m,
                VehicleNo = "MH01AB1234",
                TransDocNo = "TRDOC001",
                IsActive = 1
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = ValidCommand();
            command.Id = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            var command = ValidCommand();
            command.IsActive = 5;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_VehicleNoExceedsMaxLength_FailsValidation()
        {
            var command = ValidCommand();
            command.VehicleNo = new string('A', 21);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VehicleNo);
        }

        [Fact]
        public async Task Validate_TransDocNoExceedsMaxLength_FailsValidation()
        {
            var command = ValidCommand();
            command.TransDocNo = new string('A', 31);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TransDocNo);
        }

        [Fact]
        public async Task Validate_NegativeTotalValue_FailsValidation()
        {
            var command = ValidCommand();
            command.TotalValue = -1m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TotalValue);
        }

        [Fact]
        public async Task Validate_NegativeCGST_FailsValidation()
        {
            var command = ValidCommand();
            command.CGST = -1m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CGST);
        }

        [Fact]
        public async Task Validate_NegativeSGST_FailsValidation()
        {
            var command = ValidCommand();
            command.SGST = -1m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SGST);
        }

        [Fact]
        public async Task Validate_NegativeIGST_FailsValidation()
        {
            var command = ValidCommand();
            command.IGST = -1m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IGST);
        }

        [Fact]
        public async Task Validate_NegativeCess_FailsValidation()
        {
            var command = ValidCommand();
            command.Cess = -1m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Cess);
        }
    }
}
