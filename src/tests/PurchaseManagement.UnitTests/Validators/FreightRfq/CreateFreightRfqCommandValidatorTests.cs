using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Presentation.Validation.FreightRfq;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.FreightRfq
{
    public sealed class CreateFreightRfqCommandValidatorTests
    {
        private readonly Mock<IFreightRfqQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);

        private CreateFreightRfqCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockMiscRepo.Object);

        private void SetupHappyPath(int poBasedTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.MiscExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PurchaseOrderExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = poBasedTypeId, Code = "PO Based" });
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(FreightRfqBuilders.ValidCreateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_MissingSourceStation_Fails()
        {
            SetupHappyPath();
            var command = FreightRfqBuilders.ValidCreateCommand();
            command.SourceStation = "";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.SourceStation);
        }

        [Fact]
        public async Task Validate_ZeroTotalQuantity_Fails()
        {
            SetupHappyPath();
            var command = FreightRfqBuilders.ValidCreateCommand(totalQuantity: 0m);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.TotalQuantity);
        }

        [Fact]
        public async Task Validate_PoBasedWithoutPoReference_Fails()
        {
            SetupHappyPath(poBasedTypeId: 1);
            var command = FreightRfqBuilders.ValidCreateCommand(rfqTypeId: 1, poReferenceId: null);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.PoReferenceId);
        }

        [Fact]
        public async Task Validate_NonPoBasedWithoutPoReference_Passes()
        {
            // Misc "PO Based" resolves to id 1, but the command's type is 2 (Non-PO Based) → PO ref not required.
            SetupHappyPath(poBasedTypeId: 1);
            var command = FreightRfqBuilders.ValidCreateCommand(rfqTypeId: 2, poReferenceId: null);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.PoReferenceId);
        }
    }
}
