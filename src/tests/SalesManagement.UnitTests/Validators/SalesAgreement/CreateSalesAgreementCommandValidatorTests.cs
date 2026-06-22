using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Commands.CreateSalesAgreement;
using SalesManagement.Application.SalesAgreement.Dto;
using SalesManagement.Presentation.Validation.SalesAgreement;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesAgreement
{
    public sealed class CreateSalesAgreementCommandValidatorTests
    {
        private readonly Mock<ISalesAgreementQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateSalesAgreementCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static CreateSalesAgreementCommand ValidCommand() => new()
        {
            ValidFrom = new DateOnly(2026, 1, 1),
            ValidTo = new DateOnly(2026, 12, 31),
            CustomerId = 1,
            SalesGroupId = 1,
            PaymentTermsId = 1,
            SalesAgreementDetails = new List<CreateSalesAgreementDetailDto>
            {
                new() { ItemId = 1, AgreedRate = 10m, TotalQty = 100m }
            }
        };

        private void SetupValid()
        {
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PaymentTermsExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(1)).ReturnsAsync(true);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupValid();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ValidToBeforeValidFrom_FailsValidation()
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.ValidTo = new DateOnly(2025, 12, 31);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ValidTo);
        }

        [Fact]
        public async Task NoDetails_FailsValidation()
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.SalesAgreementDetails = new List<CreateSalesAgreementDetailDto>();
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.SalesAgreementDetails);
        }

        [Fact]
        public async Task InvalidCustomer_FailsValidation()
        {
            SetupValid();
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.CustomerId);
        }
    }
}
