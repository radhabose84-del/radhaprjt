using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;
using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Commands.CreateYarnConversionHeader;
using ProductionManagement.Presentation.Validation.YarnConversionHeader;
using ProductionManagement.UnitTests.TestHelpers;

namespace ProductionManagement.UnitTests.Validators.YarnConversionHeader
{
    public sealed class CreateYarnConversionHeaderCommandValidatorTests
    {
        private readonly Mock<IYarnConversionHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Strict);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Strict);
        private readonly Mock<IBinLookup> _mockBinLookup = new(MockBehavior.Strict);

        private CreateYarnConversionHeaderCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object,
                _mockItemLookup.Object, _mockWarehouseLookup.Object, _mockBinLookup.Object);

        private void SetupAllAsyncMocks()
        {
            _mockQueryRepo.Setup(r => r.LotMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PackTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            _mockItemLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { new() { Id = 1 } });

            _mockWarehouseLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto> { new() { Id = 1 } });

            _mockBinLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BinLookupDto> { new() { Id = 1 } });
        }

        private static CreateYarnConversionHeaderCommand BuildValidCommand() => new()
        {
            ConversionDate = DateOnly.FromDateTime(DateTime.Today),
            LotId = 1,
            OldItemId = 1,
            OldPackTypeId = 1,
            OldStartPackNo = 1,
            OldEndPackNo = 10,
            OldTotalBags = 10,
            OldNetWeightPerPack = 25m,
            OldNetWeight = 250m,
            OldWarehouseId = 1,
            OldBinId = 1,
            ItemId = 2,
            PackTypeId = 2,
            TotalBags = 5,
            NetWeightPerPack = 50m,
            NetWeight = 250m,
            LooseQty = 0m,
            WarehouseId = 2,
            BinId = 2,
            WasteQty = 0m,
            Remarks = "Test"
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(BuildValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroLotId_FailsValidation()
        {
            var command = BuildValidCommand();
            command.LotId = 0;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.LotId);
        }

        [Fact]
        public async Task Validate_ZeroItemId_FailsValidation()
        {
            var command = BuildValidCommand();
            command.ItemId = 0;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ItemId);
        }

        [Fact]
        public async Task Validate_ZeroTotalBags_FailsValidation()
        {
            var command = BuildValidCommand();
            command.TotalBags = 0;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.TotalBags);
        }

        [Fact]
        public async Task Validate_NegativeOldNetWeight_FailsValidation()
        {
            var command = BuildValidCommand();
            command.OldNetWeight = -1m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.OldNetWeight);
        }

        [Fact]
        public async Task Validate_NegativeWasteQty_FailsValidation()
        {
            var command = BuildValidCommand();
            command.WasteQty = -5m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.WasteQty);
        }

        [Fact]
        public async Task Validate_OldEndPackNo_LessThan_OldStartPackNo_FailsValidation()
        {
            var command = BuildValidCommand();
            command.OldStartPackNo = 10;
            command.OldEndPackNo = 5;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.OldEndPackNo);
        }

        [Fact]
        public async Task Validate_InvalidItemId_FK_FailsValidation()
        {
            SetupAllAsyncMocks();
            _mockItemLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>());

            var command = BuildValidCommand();
            command.ItemId = 999;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ItemId);
        }

        [Fact]
        public async Task Validate_InvalidPackTypeId_FK_FailsValidation()
        {
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.PackTypeExistsAsync(999)).ReturnsAsync(false);

            var command = BuildValidCommand();
            command.PackTypeId = 999;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.PackTypeId);
        }
    }
}
