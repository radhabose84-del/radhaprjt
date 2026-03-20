using AutoMapper;
using MediatR;
using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Application.Country.Queries.GetCountryAutoComplete;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;

namespace UserManagement.UnitTests.Application.Country.Queries
{
    public sealed class GetCountryAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ICountryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetCountryAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_MatchingResults_ReturnsList()
        {
            var entities = new List<Countries> { CountryBuilders.ValidEntity() };
            var dtos = CountryBuilders.ValidAutoCompleteList();

            _mockQueryRepo
                .Setup(r => r.GetByCountryNameAsync("India"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<CountryAutoCompleteDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCountryAutoCompleteQuery { SearchPattern = "India" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].CountryName.Should().Be("India");
        }

        [Fact]
        public async Task Handle_NoResults_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByCountryNameAsync("NonExistent"))
                .ReturnsAsync(new List<Countries>());

            Func<Task> act = async () => await CreateSut().Handle(
                new GetCountryAutoCompleteQuery { SearchPattern = "NonExistent" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No countries found*");
        }

        [Fact]
        public async Task Handle_NullSearchPattern_UsesEmptyString()
        {
            var entities = new List<Countries> { CountryBuilders.ValidEntity() };
            var dtos = CountryBuilders.ValidAutoCompleteList();

            _mockQueryRepo
                .Setup(r => r.GetByCountryNameAsync(string.Empty))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<CountryAutoCompleteDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCountryAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
