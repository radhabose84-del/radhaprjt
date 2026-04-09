using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.Common.Interfaces.ICity;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.City.Queries
{
    public sealed class GetCityQueryHandlerTests
    {
        private readonly Mock<ICityQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCityQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.Cities> { new() { Id = 1 } };
            var dtoList = new List<CityDto> { new() { Id = 1, CityName = "Test" } };

            _mockQueryRepo
                .Setup(r => r.GetAllCityAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<CityDto>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCityQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<UserManagement.Domain.Entities.Cities>();

            _mockQueryRepo
                .Setup(r => r.GetAllCityAsync(2, 5, "search"))
                .ReturnsAsync((entities, 0));

            _mockMapper
                .Setup(m => m.Map<List<CityDto>>(entities))
                .Returns(new List<CityDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCityQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }
    }
}
