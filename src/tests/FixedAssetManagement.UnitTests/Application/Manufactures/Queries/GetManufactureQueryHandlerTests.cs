using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Application.Manufacture.Queries.GetManufacture;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.Manufactures.Queries
{
    public sealed class GetManufactureQueryHandlerTests
    {
        private readonly Mock<IManufactureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ICountryLookup> _mockCountryLookup = new(MockBehavior.Loose);
        private readonly Mock<IStateLookup> _mockStateLookup = new(MockBehavior.Loose);
        private readonly Mock<ICityLookup> _mockCityLookup = new(MockBehavior.Loose);

        private GetManufactureQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockCountryLookup.Object, _mockStateLookup.Object, _mockCityLookup.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtos = new List<ManufactureDTO> { ManufacturesBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllManufactureAsync(1, 10, null))
                .ReturnsAsync((dtos, 1));

            _mockMapper
                .Setup(m => m.Map<List<ManufactureDTO>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockCountryLookup.Setup(c => c.GetByIdsAsync(It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.CountryLookupDto>());
            _mockStateLookup.Setup(s => s.GetByIdsAsync(It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.StateLookupDto>());
            _mockCityLookup.Setup(c => c.GetByIdsAsync(It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.CityLookupDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetManufactureQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtos = new List<ManufactureDTO> { ManufacturesBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllManufactureAsync(2, 5, "search"))
                .ReturnsAsync((dtos, 11));

            _mockMapper
                .Setup(m => m.Map<List<ManufactureDTO>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockCountryLookup.Setup(c => c.GetByIdsAsync(It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.CountryLookupDto>());
            _mockStateLookup.Setup(s => s.GetByIdsAsync(It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.StateLookupDto>());
            _mockCityLookup.Setup(c => c.GetByIdsAsync(It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.CityLookupDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetManufactureQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllManufactureAsync(1, 10, null))
                .ReturnsAsync((new List<ManufactureDTO>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<ManufactureDTO>>(It.IsAny<object>()))
                .Returns(new List<ManufactureDTO>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetManufactureQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
