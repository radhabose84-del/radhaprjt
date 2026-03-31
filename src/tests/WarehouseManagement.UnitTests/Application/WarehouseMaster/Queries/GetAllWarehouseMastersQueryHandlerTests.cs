using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.WarehouseMaster.Queries
{
    public sealed class GetAllWarehouseMastersQueryHandlerTests
    {
        private readonly Mock<IWarehouseMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterLookup> _mockMiscLookup = new(MockBehavior.Loose);
        private readonly Mock<ICityLookup> _mockCityLookup = new(MockBehavior.Loose);
        private readonly Mock<ICountryLookup> _mockCountryLookup = new(MockBehavior.Loose);
        private readonly Mock<IStateLookup> _mockStateLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetAllWarehouseMastersQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockMiscLookup.Object, _mockUomLookup.Object, _mockCityLookup.Object,
                _mockCountryLookup.Object, _mockStateLookup.Object, _mockDeptLookup.Object);

        private void SetupLookups()
        {
            _mockUomLookup.Setup(l => l.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>());
            _mockCityLookup.Setup(l => l.GetAllCityAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CityLookupDto>());
            _mockStateLookup.Setup(l => l.GetAllStatesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StateLookupDto>());
            _mockCountryLookup.Setup(l => l.GetAllCountriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CountryLookupDto>());
            _mockDeptLookup.Setup(l => l.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>());
            _mockMiscLookup.Setup(l => l.GetMiscMasterByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<MiscMasterLookupDto>());
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            SetupLookups();
            var dtos = new List<WarehouseMasterDto> { WarehouseMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null!)).ReturnsAsync((dtos, 1));
            _mockMapper.Setup(m => m.Map<List<WarehouseMasterDto>>(It.IsAny<object>()))
                .Returns(new List<WarehouseMasterDto> { WarehouseMasterBuilders.ValidDto() });

            var result = await CreateSut().Handle(
                new GetAllWarehouseMastersQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            SetupLookups();
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null!))
                .ReturnsAsync((new List<WarehouseMasterDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<WarehouseMasterDto>>(It.IsAny<object>()))
                .Returns(new List<WarehouseMasterDto>());

            var result = await CreateSut().Handle(
                new GetAllWarehouseMastersQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            SetupLookups();
            var dtos = new List<WarehouseMasterDto> { WarehouseMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "search")).ReturnsAsync((dtos, 11));
            _mockMapper.Setup(m => m.Map<List<WarehouseMasterDto>>(It.IsAny<object>()))
                .Returns(new List<WarehouseMasterDto> { WarehouseMasterBuilders.ValidDto() });

            var result = await CreateSut().Handle(
                new GetAllWarehouseMastersQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }
    }
}
