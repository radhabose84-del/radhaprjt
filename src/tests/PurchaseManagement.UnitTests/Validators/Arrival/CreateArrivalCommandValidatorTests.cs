using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using FluentValidation.TestHelper;
using PurchaseManagement.Application.Arrival.Commands.CreateArrival;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Presentation.Validation.Arrival;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.Arrival
{
    public sealed class CreateArrivalCommandValidatorTests
    {
        private readonly Mock<IArrivalQueryRepository> _queryRepo = new(MockBehavior.Loose);
        private readonly Mock<ISupplierLookup> _supplier = new(MockBehavior.Loose);
        private readonly Mock<IStationLookup> _station = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _warehouse = new(MockBehavior.Loose);
        private readonly Mock<ITransporterLookup> _transporter = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _item = new(MockBehavior.Loose);
        private readonly Mock<IHSNLookup> _hsn = new(MockBehavior.Loose);
        private readonly Mock<IPackTypeLookup> _packType = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _uom = new(MockBehavior.Loose);

        private CreateArrivalCommandValidator CreateValidator() =>
            new(_queryRepo.Object, _supplier.Object, _station.Object, _warehouse.Object,
                _transporter.Object, _item.Object, _hsn.Object, _packType.Object, _uom.Object);

        private void SetupAllValid()
        {
            _queryRepo.Setup(r => r.RawMaterialPOExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _queryRepo.Setup(r => r.GetRawMaterialPOItemQuantitiesAsync(It.IsAny<int>()))
                .ReturnsAsync(new Dictionary<int, decimal> { [1] = 500m });

            _supplier.Setup(s => s.GetActiveSupplierByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SupplierLookupDto { Id = 1, VendorName = "Sree Lakshmi" });
            _transporter.Setup(t => t.GetActiveTransporterByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransporterLookupDto { Id = 1, TransporterName = "TCI" });
            _station.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StationLookupDto> { new() { Id = 1 } });
            _warehouse.Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto> { new() { Id = 1 } });
            _item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { new() { Id = 1 } });
            _hsn.Setup(h => h.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HSNLookupDto> { new() { Id = 1 } });
            _packType.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PackTypeLookupDto> { new() { Id = 1 } });
            _uom.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto> { new() { Id = 1 } });
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllValid();
            var result = await CreateValidator().TestValidateAsync(ArrivalBuilders.ValidCreateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyVehicleNumber_FailsValidation()
        {
            SetupAllValid();
            var command = ArrivalBuilders.ValidCreateCommand();
            command.VehicleNumber = "";

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.VehicleNumber);
        }

        [Fact]
        public async Task Validate_MissingTareWeight_FailsValidation()
        {
            SetupAllValid();
            var command = ArrivalBuilders.ValidCreateCommand();
            command.TareWeight = 0m;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.TareWeight);
        }

        [Fact]
        public async Task Validate_OverlappingBaleRange_WithinPayload_FailsValidation()
        {
            SetupAllValid();
            // Two lines in the SAME arrival (lotno) whose bale ranges overlap → rejected.
            var command = ArrivalBuilders.ValidCreateCommand();
            command.Details.Add(new CreateArrivalDetailDto
            {
                ItemId = 1, HsnId = 1, PackTypeId = 1, MixCodeId = 1, UomId = 1,
                Rate = 68500m, OrderedQty = 500m, ArrivedQty = 150m, CancelledQty = 0m,
                BatchNumber = "BATCH-0012-B",
                BaleNumberFrom = 100100, BaleNumberTo = 100200, TotalBaleCount = 101
            });

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_ArrivedQtyExceedsPoQuantity_FailsValidation()
        {
            SetupAllValid();
            // PO ordered qty for item 1 is 500 (see SetupAllValid). Arrive 600 → rejected.
            var command = ArrivalBuilders.ValidCreateCommand();
            command.Details[0].ArrivedQty = 600m;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_ArrivedQtyEqualsPoQuantity_PassesValidation()
        {
            SetupAllValid();
            // Arriving exactly the PO ordered qty (500) is allowed.
            var command = ArrivalBuilders.ValidCreateCommand();
            command.Details[0].ArrivedQty = 500m;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_MissingGstPercentage_FailsValidation()
        {
            SetupAllValid();
            var command = ArrivalBuilders.ValidCreateCommand();
            command.GstPercentage = null;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GstPercentage);
        }

        [Fact]
        public async Task Validate_GstPercentageAbove100_FailsValidation()
        {
            SetupAllValid();
            var command = ArrivalBuilders.ValidCreateCommand();
            command.GstPercentage = 150m;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GstPercentage);
        }

        [Fact]
        public async Task Validate_NonOverlappingBaleRanges_WithinPayload_PassesValidation()
        {
            SetupAllValid();
            // Two lines with disjoint bale ranges in the same arrival → allowed.
            var command = ArrivalBuilders.ValidCreateCommand();
            command.Details.Add(new CreateArrivalDetailDto
            {
                ItemId = 1, HsnId = 1, PackTypeId = 1, MixCodeId = 1, UomId = 1,
                Rate = 68500m, OrderedQty = 500m, ArrivedQty = 150m, CancelledQty = 0m,
                BatchNumber = "BATCH-0012-B",
                BaleNumberFrom = 100151, BaleNumberTo = 100200, TotalBaleCount = 50
            });

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
