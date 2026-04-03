using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;
using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.CreateRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.UpdateRepackingHeader;
using ProductionManagement.Presentation.Validation.RepackingHeader;
using ProductionManagement.UnitTests.TestHelpers;

namespace ProductionManagement.UnitTests.Validators.Repacking
{
    public sealed class UpdateRepackingHeaderCommandValidatorTests
    {
        private readonly Mock<IRepackingHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Strict);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Strict);
        private readonly Mock<IBinLookup> _mockBinLookup = new(MockBehavior.Strict);

        private UpdateRepackingHeaderCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object,
                _mockItemLookup.Object, _mockWarehouseLookup.Object, _mockBinLookup.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);

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

        private static UpdateRepackingHeaderCommand BuildValidCommand(int id = 1) => new()
        {
            Id = id,
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
            Remarks = "Updated",
            IsActive = 1,
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
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = BuildValidCommand(0);
            SetupAllAsyncMocks(0);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            SetupAllAsyncMocks(999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(BuildValidCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            var command = BuildValidCommand();
            command.IsActive = 5;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
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
        public async Task Validate_NegativeNetWeight_FailsValidation()
        {
            var command = BuildValidCommand();
            command.NetWeight = -10m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.NetWeight);
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
