using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.CreateRawMaterialPO;
using PurchaseManagement.Presentation.Validation.RawMaterialPO;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.RawMaterialPO
{
    public sealed class CreateRawMaterialPOCommandValidatorTests
    {
        private readonly Mock<IRawMaterialPOQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItem = new(MockBehavior.Loose);
        private readonly Mock<IHSNLookup> _mockHsn = new(MockBehavior.Loose);

        private CreateRawMaterialPOCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockItem.Object, _mockHsn.Object);

        private void SetupValid(decimal ocrQty = 800m, decimal alreadyConverted = 0m)
        {
            _mockQueryRepo.Setup(r => r.OcrExistsAndApprovedAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetOcrQuantityAsync(It.IsAny<int>())).ReturnsAsync(ocrQty);
            _mockQueryRepo.Setup(r => r.GetConvertedQuantityAsync(It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(alreadyConverted);
            _mockItem.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { new() { Id = 1, ItemName = "Cotton" } });
            _mockHsn.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(RawMaterialPOBuilders.HsnList());
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupValid();
            var result = await CreateValidator().TestValidateAsync(RawMaterialPOBuilders.ValidCreateCommand(quantity: 500m));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_OcrNotApproved_Fails()
        {
            SetupValid();
            _mockQueryRepo.Setup(r => r.OcrExistsAndApprovedAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(RawMaterialPOBuilders.ValidCreateCommand());
            result.ShouldHaveValidationErrorFor(x => x.OcrId);
        }

        [Fact]
        public async Task Validate_OverConversion_Fails()
        {
            // 500 requested + 400 already = 900 > 800 OCR total
            SetupValid(ocrQty: 800m, alreadyConverted: 400m);

            var result = await CreateValidator().TestValidateAsync(RawMaterialPOBuilders.ValidCreateCommand(quantity: 500m));
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_EmptyDetails_Fails()
        {
            SetupValid();
            var command = RawMaterialPOBuilders.ValidCreateCommand();
            command.Details.Clear();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Details);
        }
    }
}
