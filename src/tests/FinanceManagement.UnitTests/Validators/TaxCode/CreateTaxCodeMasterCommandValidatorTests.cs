using Contracts.Interfaces;
using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeMaster;
using FinanceManagement.Presentation.Validation.TaxCode;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.TaxCode
{
    public sealed class CreateTaxCodeMasterCommandValidatorTests
    {
        private readonly Mock<ITaxCodeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        // MiscMaster ids -> codes (resolved by the validator for business-rule branching)
        // 10=GST_OUT, 13=TDS, 20=COMBINED, 21=CGST, 30=OUTPUT
        public CreateTaxCodeMasterCommandValidatorTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.TaxTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.TaxComponentExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.DirectionExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.TaxCodeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.TaxCodeAlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetMiscCodeAsync(It.IsAny<int>())).ReturnsAsync((int id) => id switch
            {
                10 => "GST_OUT",
                13 => "TDS",
                20 => "COMBINED",
                21 => "CGST",
                30 => "OUTPUT",
                _ => null
            });
        }

        private CreateTaxCodeMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockIp.Object);

        private static CreateTaxCodeMasterCommand ValidCommand() =>
            new()
            {
                TaxCode = "GST-OUT-5",
                TaxName = "GST Output 5%",
                TaxTypeId = 10,
                TaxComponentId = 20,
                DirectionId = 30,
                RatePercent = 5.0m,
                EffectiveFrom = new DateOnly(2026, 6, 16)
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyTaxCode_FailsValidation(string? code)
        {
            var command = ValidCommand();
            command.TaxCode = code;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.TaxCode);
        }

        [Fact]
        public async Task Validate_ZeroTaxTypeId_FailsValidation()
        {
            var command = ValidCommand();
            command.TaxTypeId = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.TaxTypeId);
        }

        [Fact]
        public async Task Validate_DuplicateTaxCode_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.TaxCodeAlreadyExistsAsync("GST-OUT-5", 1, null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldHaveValidationErrorFor(x => x.TaxCode);
        }

        [Fact]
        public async Task Validate_TdsWithoutStatutorySection_FailsValidation()
        {
            var command = ValidCommand();
            command.TaxTypeId = 13;       // TDS
            command.DirectionId = null;
            command.StatutorySection = null;
            command.RatePercent = 1.0m;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.StatutorySection);
        }

        [Fact]
        public async Task Validate_GstWithoutRate_FailsValidation()
        {
            var command = ValidCommand();
            command.RatePercent = 0m;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RatePercent);
        }

        [Fact]
        public async Task Validate_ComponentChildWithoutParent_FailsValidation()
        {
            var command = ValidCommand();
            command.TaxComponentId = 21;   // CGST
            command.ParentTaxCodeId = null;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ParentTaxCodeId);
        }
    }
}
