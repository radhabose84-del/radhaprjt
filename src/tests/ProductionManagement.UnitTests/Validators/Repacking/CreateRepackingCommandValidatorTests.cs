using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;
using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Commands.CreateRepackingMaster;
using ProductionManagement.Presentation.Validation.RepackingMaster;
using ProductionManagement.UnitTests.TestHelpers;

namespace ProductionManagement.UnitTests.Validators.Repacking
{
    public sealed class CreateRepackingMasterCommandValidatorTests
    {
        private readonly Mock<IRepackingMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Strict);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Strict);
        private readonly Mock<IBinLookup> _mockBinLookup = new(MockBehavior.Strict);

        private CreateRepackingMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object,
                _mockItemLookup.Object, _mockWarehouseLookup.Object, _mockBinLookup.Object);

        private void SetupAllAsyncMocks()
        {
            _mockItemLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { new() { Id = 1 } });

            _mockQueryRepo.Setup(r => r.PackTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            _mockWarehouseLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto> { new() { Id = 1 } });

            _mockBinLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BinLookupDto> { new() { Id = 1 } });
        }

        private static CreateRepackingMasterCommand BuildValidCommand() => new()
        {
            RepackDate = DateOnly.FromDateTime(DateTime.Today),
            ItemId = 1,
            OldPackTypeId = 1,
            OldNetWeightPerPack = 50m,
            OldStartPackNo = 1,
            OldEndPackNo = 10,
            OldTotalBags = 10,
            OldNetWeight = 500m,
            OldWarehouseId = 1,
            OldBinId = 1,
            PackTypeId = 2,
            NetWeightPerPack = 25m,
            TotalBags = 20,
            NetWeight = 500m,
            WarehouseId = 2,
            BinId = 2,
            LooseConeKgs = 0m,
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
        public async Task Validate_ZeroItemId_FailsValidation()
        {
            var command = BuildValidCommand();
            command.ItemId = 0;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ItemId);
        }

        [Fact]
        public async Task Validate_ZeroOldPackTypeId_FailsValidation()
        {
            var command = BuildValidCommand();
            command.OldPackTypeId = 0;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.OldPackTypeId);
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
        public async Task Validate_NegativeLooseConeKgs_FailsValidation()
        {
            var command = BuildValidCommand();
            command.LooseConeKgs = -5m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.LooseConeKgs);
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

        [Fact]
        public async Task Validate_InvalidWarehouseId_FK_FailsValidation()
        {
            SetupAllAsyncMocks();
            _mockWarehouseLookup
                .Setup(l => l.GetByIdsAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(999)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto>());

            var command = BuildValidCommand();
            command.WarehouseId = 999;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.WarehouseId);
        }
    }
}
