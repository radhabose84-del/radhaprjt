using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Presentation.Validation.VoucherType;
using FinanceManagement.UnitTests.TestData;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.VoucherType
{
    public sealed class CreateVoucherTypeCommandValidatorTests
    {
        private readonly Mock<IVoucherTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateVoucherTypeCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockIp.Object);

        private void SetupHappyPath()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.AccountTypeExistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(VoucherTypeBuilders.ValidCreateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_Fails(string? code)
        {
            SetupHappyPath();
            var cmd = VoucherTypeBuilders.ValidCreateCommand();
            cmd.VoucherTypeCode = code;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.VoucherTypeCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_Fails(string? name)
        {
            SetupHappyPath();
            var cmd = VoucherTypeBuilders.ValidCreateCommand();
            cmd.VoucherTypeName = name;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.VoucherTypeName);
        }

        [Theory]
        [InlineData("JV-1")]   // hyphen
        [InlineData("JV 1")]   // space
        [InlineData("JV@1")]   // special char
        public async Task Validate_NonAlphanumericCode_Fails(string code)
        {
            SetupHappyPath();
            var cmd = VoucherTypeBuilders.ValidCreateCommand();
            cmd.VoucherTypeCode = code;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.VoucherTypeCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        public async Task Validate_NumberPaddingOutOfRange_Fails(int padding)
        {
            SetupHappyPath();
            var cmd = VoucherTypeBuilders.ValidCreateCommand();
            cmd.NumberPadding = padding;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.NumberPadding);
        }

        [Fact]
        public async Task Validate_NoAccountTypes_Fails()
        {
            SetupHappyPath();
            var cmd = VoucherTypeBuilders.ValidCreateCommand(accountTypeIds: new List<int>());

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.AllowedAccountTypeIds);
        }

        [Fact]
        public async Task Validate_DuplicateCode_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync("JV", 1, It.IsAny<int?>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(VoucherTypeBuilders.ValidCreateCommand());
            result.ShouldHaveValidationErrorFor(x => x.VoucherTypeCode);
        }

        [Fact]
        public async Task Validate_NonExistentAccountType_Fails()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AccountTypeExistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(VoucherTypeBuilders.ValidCreateCommand());
            result.IsValid.Should().BeFalse();
        }
    }
}
