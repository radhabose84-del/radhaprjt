using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Commands.DeleteVoucherType;
using FinanceManagement.Presentation.Validation.VoucherType;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.VoucherType
{
    public sealed class DeleteVoucherTypeCommandValidatorTests
    {
        private readonly Mock<IVoucherTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteVoucherTypeCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsSystemAsync(It.IsAny<int>())).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidId_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteVoucherTypeCommand(5));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NonExistentId_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteVoucherTypeCommand(99));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ConsumedSeries_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(5)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteVoucherTypeCommand(5));
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Validate_SystemVoucherType_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.IsSystemAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteVoucherTypeCommand(1));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
