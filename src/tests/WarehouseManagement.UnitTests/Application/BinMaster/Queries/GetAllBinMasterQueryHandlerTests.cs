using Contracts.Common;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.BinMaster.Queries
{
    public sealed class GetAllBinMasterQueryHandlerTests
    {
        private readonly Mock<IBinMasterQueryRepository> _mockBinRepo = new(MockBehavior.Strict);
        private readonly Mock<IWarehouseMasterQueryRepository> _mockWhRepo = new(MockBehavior.Loose);
        private readonly Mock<IRackMasterQueryRepository> _mockRackRepo = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllBinMasterQueryHandler CreateSut() =>
            new(_mockBinRepo.Object, _mockWhRepo.Object, _mockRackRepo.Object, _mockUomLookup.Object, _mockMediator.Object);

        private void SetupLookups()
        {
            _mockUomLookup.Setup(l => l.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UOMLookupDto>());
            _mockWhRepo.Setup(r => r.GetwarehouseAsync()).ReturnsAsync(new List<WarehouseMasterDto>());
            _mockRackRepo.Setup(r => r.GetRackAsync()).ReturnsAsync(new List<RackMasterDto>());
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            SetupLookups();
            var bins = new List<BinMasterDto> { BinMasterBuilders.ValidDto() };
            _mockBinRepo.Setup(r => r.GetAllAsync(1, 10, "")).ReturnsAsync((bins, 1));

            var result = await CreateSut().Handle(new GetAllBinMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockBinRepo.Setup(r => r.GetAllAsync(1, 10, "")).ReturnsAsync((new List<BinMasterDto>(), 0));

            var result = await CreateSut().Handle(new GetAllBinMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            SetupLookups();
            var bins = new List<BinMasterDto> { BinMasterBuilders.ValidDto() };
            _mockBinRepo.Setup(r => r.GetAllAsync(2, 5, "x")).ReturnsAsync((bins, 20));

            var result = await CreateSut().Handle(
                new GetAllBinMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "x" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(20);
        }
    }
}
