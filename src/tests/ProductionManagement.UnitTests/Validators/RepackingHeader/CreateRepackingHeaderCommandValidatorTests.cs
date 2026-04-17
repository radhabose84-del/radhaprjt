using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;
using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.CreateRepackingHeader;
using ProductionManagement.Presentation.Validation.RepackingHeader;
using ProductionManagement.UnitTests.TestHelpers;

namespace ProductionManagement.UnitTests.Validators.RepackingHeader
{
    public sealed class CreateRepackingHeaderCommandValidatorTests
    {
        private readonly Mock<IRepackingHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Strict);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Strict);
        private readonly Mock<IBinLookup> _mockBinLookup = new(MockBehavior.Strict);

        private CreateRepackingHeaderCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object,
                _mockItemLookup.Object, _mockWarehouseLookup.Object, _mockBinLookup.Object);

        private void SetupAllAsyncMocks()
        {
            _mockItemLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { new() { Id = 1 } });

            _mockQueryRepo.Setup(r => r.PackTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.LotMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            _mockWarehouseLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto> { new() { Id = 1 } });

            _mockBinLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BinLookupDto> { new() { Id = 1 } });
        }

        private static CreateRepackingHeaderCommand BuildValidCommand() => new()
        {
            RepackDate = DateOnly.FromDateTime(DateTime.Today),
            ItemId = 1,
            OldItemId = 1,
            OldPackTypeId = 1,
            PackTypeId = 2,
            NetWeightPerPack = 25m,
            TotalBags = 20,
            NetWeight = 500m,
            WarehouseId = 1,
            BinId = 1,
            LooseConeKgs = 0m,
            Remarks = "Test",
            Details = new List<CreateRepackingDetailItem>
            {
                new() { OldStartPackNo = 1, OldEndPackNo = 10 }
            }
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
        public async Task Validate_NegativeLooseConeKgs_FailsValidation()
        {
            var command = BuildValidCommand();
            command.LooseConeKgs = -5m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.LooseConeKgs);
        }

        [Fact]
        public async Task Validate_NegativeNetWeight_FailsValidation()
        {
            var command = BuildValidCommand();
            command.NetWeight = -10m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.NetWeight);
        }

        [Fact]
        public async Task Validate_EmptyDetails_FailsValidation()
        {
            var command = BuildValidCommand();
            command.Details = new List<CreateRepackingDetailItem>();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Details);
        }

        [Fact]
        public async Task Validate_OldEndPackNo_LessThan_OldStartPackNo_FailsValidation()
        {
            var command = BuildValidCommand();
            command.Details[0].OldStartPackNo = 10;
            command.Details[0].OldEndPackNo = 5;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor("Details[0].OldEndPackNo");
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

        [Fact]
        public async Task Validate_InvalidBinId_FK_FailsValidation()
        {
            SetupAllAsyncMocks();
            _mockBinLookup
                .Setup(l => l.GetByIdsAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(999)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BinLookupDto>());

            var command = BuildValidCommand();
            command.BinId = 999;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.BinId);
        }
    }
}
