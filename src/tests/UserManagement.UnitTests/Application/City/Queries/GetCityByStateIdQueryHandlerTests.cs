using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.City.Queries.GetCityByStateId;
using UserManagement.Application.Common.Interfaces.ICity;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.City.Queries
{
    public sealed class GetCityByStateIdQueryHandlerTests
    {
        private readonly Mock<ICityQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCityByStateIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidStateId_ReturnsCities()
        {
            var entities = new List<UserManagement.Domain.Entities.Cities> { new() { Id = 1 } };
            var dtoList = new List<CityDto> { new() { Id = 1, CityName = "TestCity" } };

            _mockQueryRepo
                .Setup(r => r.GetCityByStateIdAsync(5))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<CityDto>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCityByStateIdQuery { Id = 5 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NoCitiesFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetCityByStateIdAsync(999))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.Cities>());

            Func<Task> act = () => CreateSut().Handle(
                new GetCityByStateIdQuery { Id = 999 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
