using AutoMapper;
using MediatR;
using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.Country.Queries
{
    public sealed class GetAllCountryQueryHandlerTests
    {
        private readonly Mock<ICountryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetCountryQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<Countries> { CountryBuilders.ValidEntity() };
            var dtos = new List<CountryDto> { CountryBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllCountriesAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<CountryDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCountryQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<Countries> { CountryBuilders.ValidEntity() };
            var dtos = new List<CountryDto> { CountryBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllCountriesAsync(2, 5, "test"))
                .ReturnsAsync((entities, 11));

            _mockMapper
                .Setup(m => m.Map<List<CountryDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCountryQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyEntities = new List<Countries>();
            var emptyDtos = new List<CountryDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllCountriesAsync(1, 10, null))
                .ReturnsAsync((emptyEntities, 0));

            _mockMapper
                .Setup(m => m.Map<List<CountryDto>>(emptyEntities))
                .Returns(emptyDtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCountryQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var entities = new List<Countries> { CountryBuilders.ValidEntity() };
            var dtos = new List<CountryDto> { CountryBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllCountriesAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<CountryDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetCountryQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAll" &&
                        e.Module == "Country"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
