using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.ILocation;
using UserManagement.Application.Location.Queries.GetLocationAutoSearch;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Location.Queries
{
    public sealed class GetLocationAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ILocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetLocationAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static List<UserManagement.Domain.Entities.Location> ValidEntityList() =>
            new()
            {
                new() { Id = 1, Code = "LOC-0001", LocationName = "Loc Alpha" },
                new() { Id = 2, Code = "LOC-0002", LocationName = "Loc Beta" }
            };

        private static List<LocationAutoCompleteDto> ValidDtoList() =>
            new()
            {
                new() { Id = 1, Code = "LOC-0001", LocationName = "Loc Alpha" },
                new() { Id = 2, Code = "LOC-0002", LocationName = "Loc Beta" }
            };

        [Fact]
        public async Task Handle_MatchingPattern_ReturnsDtoList()
        {
            var entities = ValidEntityList();
            var query = new GetLocationAutoCompleteQuery { SearchPattern = "Loc" };

            _mockQueryRepo.Setup(r => r.GetAllLocationAsync("Loc")).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<LocationAutoCompleteDto>>(entities)).Returns(ValidDtoList());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NullPattern_UsesEmptyString()
        {
            var entities = ValidEntityList();
            var query = new GetLocationAutoCompleteQuery { SearchPattern = null };

            _mockQueryRepo.Setup(r => r.GetAllLocationAsync(string.Empty)).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<LocationAutoCompleteDto>>(entities)).Returns(ValidDtoList());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NoMatchingPattern_ThrowsValidationException()
        {
            var query = new GetLocationAutoCompleteQuery { SearchPattern = "ZZZNOMATCH" };
            _mockQueryRepo.Setup(r => r.GetAllLocationAsync("ZZZNOMATCH")).ReturnsAsync(new List<UserManagement.Domain.Entities.Location>());

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*No Location found*");
        }
    }
}
