using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Cancel;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.ContractPO;

namespace PurchaseManagement.UnitTests.Validators.ContractPO
{
    public sealed class CancelContractPOCommandValidatorTests
    {
        private readonly Mock<IContractPOQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CancelContractPOCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        // Cancel requires: exists AND no GRN.
        private void SetupValid()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.HasAnyGrnAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupValid();
            var result = await CreateValidator().TestValidateAsync(new CancelContractPOCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            SetupValid();
            var result = await CreateValidator().TestValidateAsync(new CancelContractPOCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.HasAnyGrnAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new CancelContractPOCommand(1));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
