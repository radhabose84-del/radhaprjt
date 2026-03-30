using AutoMapper;
using MediatR;
using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.City.Queries.GetCityAutoComplete;
using UserManagement.Application.Common.Interfaces.ICity;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;

namespace UserManagement.UnitTests.Application.City.Queries
{
    public sealed class GetCityAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ICityQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetCityAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_MatchingResults_ReturnsList()
        {
            var entities = new List<Cities> { CityBuilders.ValidEntity() };
            var dtos = CityBuilders.ValidAutoCompleteList();

            _mockQueryRepo
                .Setup(r => r.GetByCityNameAsync("Test"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<CityAutoCompleteDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCityAutoCompleteQuery { SearchPattern = "Test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].CityName.Should().Be("Test City");
        }

        [Fact]
        public async Task Handle_NoResults_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByCityNameAsync("NonExistent"))
                .ReturnsAsync(new List<Cities>());

            Func<Task> act = async () => await CreateSut().Handle(
                new GetCityAutoCompleteQuery { SearchPattern = "NonExistent" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No Cities found*");
        }

        [Fact]
        public async Task Handle_NullSearchPattern_UsesEmptyString()
        {
            var entities = new List<Cities> { CityBuilders.ValidEntity() };
            var dtos = CityBuilders.ValidAutoCompleteList();

            _mockQueryRepo
                .Setup(r => r.GetByCityNameAsync(string.Empty))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<CityAutoCompleteDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCityAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
