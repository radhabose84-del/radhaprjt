using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.RackMaster.Queries
{
    public sealed class GetAllRackMasterQueryHandlerTests
    {
        private readonly Mock<IRackMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterLookup> _mockMiscLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        private GetAllRackMasterQueryHanlder CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockMiscLookup.Object, _mockUomLookup.Object);

        private void SetupLookups()
        {
            _mockUomLookup.Setup(l => l.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UOMLookupDto>());
            _mockMiscLookup.Setup(l => l.GetMiscMasterByIdAsync(It.IsAny<string>())).ReturnsAsync(new List<MiscMasterLookupDto>());
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            SetupLookups();
            var dtos = new List<RackMasterDto> { RackMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtos, 1));
            _mockMapper.Setup(m => m.Map<List<RackMasterDto>>(It.IsAny<object>()))
                .Returns(new List<RackMasterDto> { RackMasterBuilders.ValidDto() });

            var result = await CreateSut().Handle(new GetAllRackMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            SetupLookups();
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<RackMasterDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<RackMasterDto>>(It.IsAny<object>())).Returns(new List<RackMasterDto>());

            var result = await CreateSut().Handle(new GetAllRackMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            SetupLookups();
            var dtos = new List<RackMasterDto> { RackMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "x")).ReturnsAsync((dtos, 20));
            _mockMapper.Setup(m => m.Map<List<RackMasterDto>>(It.IsAny<object>()))
                .Returns(new List<RackMasterDto> { RackMasterBuilders.ValidDto() });

            var result = await CreateSut().Handle(new GetAllRackMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "x" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(20);
        }
    }
}
