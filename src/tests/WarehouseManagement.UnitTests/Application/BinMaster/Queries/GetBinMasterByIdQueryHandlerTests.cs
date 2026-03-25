using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster;
using WarehouseManagement.Application.BinMaster.Queries.GetBinMasterById;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.BinMaster.Queries
{
    public sealed class GetBinMasterByIdQueryHandlerTests
    {
        private readonly Mock<IBinMasterQueryRepository> _mockBinRepo = new(MockBehavior.Strict);
        private readonly Mock<IWarehouseMasterQueryRepository> _mockWhRepo = new(MockBehavior.Loose);
        private readonly Mock<IRackMasterQueryRepository> _mockRackRepo = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterLookup> _mockMiscLookup = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetBinMasterByIdQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockBinRepo.Object, _mockMediator.Object,
                _mockWhRepo.Object, _mockRackRepo.Object, _mockUomLookup.Object, _mockMiscLookup.Object);

        private void SetupLookups()
        {
            _mockUomLookup.Setup(l => l.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UOMLookupDto>());
            _mockWhRepo.Setup(r => r.GetwarehouseAsync()).ReturnsAsync(new List<WarehouseMasterDto>());
            _mockRackRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((RackMasterDto?)null);
        }

        [Fact]
        public async Task Handle_Found_ReturnsDto()
        {
            SetupLookups();
            var dto = BinMasterBuilders.ValidDto();
            _mockBinRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<BinMasterDto>(It.IsAny<object>())).Returns(dto);

            var result = await CreateSut().Handle(new GetBinMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockBinRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((BinMasterDto)null!);

            Func<Task> act = async () => await CreateSut().Handle(new GetBinMasterByIdQuery { Id = 999 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }
    }
}
