using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.GetWarehouseMasterById;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.WarehouseMaster.Queries
{
    public sealed class GetWarehouseMasterByIdQueryHandlerTests
    {
        private readonly Mock<IWarehouseMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterLookup> _mockMiscLookup = new(MockBehavior.Loose);
        private readonly Mock<ICityLookup> _mockCityLookup = new(MockBehavior.Loose);
        private readonly Mock<ICountryLookup> _mockCountryLookup = new(MockBehavior.Loose);
        private readonly Mock<IStateLookup> _mockStateLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetWarehouseMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMiscLookup.Object, _mockMapper.Object,
                _mockUomLookup.Object, _mockCityLookup.Object, _mockCountryLookup.Object,
                _mockStateLookup.Object, _mockDeptLookup.Object);

        private void SetupLookups()
        {
            _mockUomLookup.Setup(l => l.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UOMLookupDto>());
            _mockCityLookup.Setup(l => l.GetAllCityAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<CityLookupDto>());
            _mockStateLookup.Setup(l => l.GetAllStatesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<StateLookupDto>());
            _mockCountryLookup.Setup(l => l.GetAllCountriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<CountryLookupDto>());
            _mockDeptLookup.Setup(l => l.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockMiscLookup.Setup(l => l.GetMiscMasterByIdAsync(It.IsAny<string>())).ReturnsAsync(new List<MiscMasterLookupDto>());
        }

        [Fact]
        public async Task Handle_Found_ReturnsSuccess()
        {
            SetupLookups();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(WarehouseMasterBuilders.ValidDto());
            _mockMapper.Setup(m => m.Map<WarehouseMasterDto>(It.IsAny<object>())).Returns(WarehouseMasterBuilders.ValidDto());

            var result = await CreateSut().Handle(new GetWarehouseMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((WarehouseMasterDto)null!);

            var result = await CreateSut().Handle(new GetWarehouseMasterByIdQuery { Id = 999 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
