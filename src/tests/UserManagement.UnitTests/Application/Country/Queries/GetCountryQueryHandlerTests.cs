using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Country.Queries
{
    public sealed class GetCountryQueryHandlerTests
    {
        private readonly Mock<ICountryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCountryQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.Countries> { new() { Id = 1 } };
            var dtoList = new List<CountryDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllCountriesAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<CountryDto>>(entities))
                .Returns(dtoList);

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
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllCountriesAsync(1, 10, null))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.Countries>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<CountryDto>>(It.IsAny<object>()))
                .Returns(new List<CountryDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCountryQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
