using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Presentation.Validation.VoucherType;
using FinanceManagement.UnitTests.TestData;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.VoucherType
{
    public sealed class UpdateVoucherTypeCommandValidatorTests
    {
        private readonly Mock<IVoucherTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateVoucherTypeCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupHappyPath() =>
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(VoucherTypeBuilders.ValidUpdateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_Fails(string? name)
        {
            SetupHappyPath();
            var cmd = VoucherTypeBuilders.ValidUpdateCommand();
            cmd.VoucherTypeName = name;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.VoucherTypeName);
        }

        [Fact]
        public async Task Validate_NonExistentId_Fails()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var cmd = VoucherTypeBuilders.ValidUpdateCommand(id: 99);

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task Validate_InvalidIsActive_Fails(int isActive)
        {
            SetupHappyPath();
            var cmd = VoucherTypeBuilders.ValidUpdateCommand(isActive: isActive);

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_NoAccountTypes_Fails()
        {
            SetupHappyPath();
            var cmd = VoucherTypeBuilders.ValidUpdateCommand(accountTypeIds: new List<int>());

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.AllowedAccountTypeIds);
        }
    }
}
