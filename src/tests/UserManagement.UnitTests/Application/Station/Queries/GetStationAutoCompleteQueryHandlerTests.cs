using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IStation;
using UserManagement.Application.Station.Queries.GetStationAutoSearch;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Station.Queries
{
    public sealed class GetStationAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IStationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetStationAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static List<UserManagement.Domain.Entities.Station> ValidEntityList() =>
            new()
            {
                new() { Id = 1, Code = "STA-0001", StationName = "Station Alpha" },
                new() { Id = 2, Code = "STA-0002", StationName = "Station Beta" }
            };

        private static List<StationAutoCompleteDto> ValidDtoList() =>
            new()
            {
                new() { Id = 1, Code = "STA-0001", StationName = "Station Alpha" },
                new() { Id = 2, Code = "STA-0002", StationName = "Station Beta" }
            };

        [Fact]
        public async Task Handle_MatchingPattern_ReturnsDtoList()
        {
            var entities = ValidEntityList();
            var query = new GetStationAutoCompleteQuery { SearchPattern = "Station" };

            _mockQueryRepo.Setup(r => r.GetAllStationAsync("Station")).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<StationAutoCompleteDto>>(entities)).Returns(ValidDtoList());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NullPattern_UsesEmptyString()
        {
            var entities = ValidEntityList();
            var query = new GetStationAutoCompleteQuery { SearchPattern = null };

            _mockQueryRepo.Setup(r => r.GetAllStationAsync(string.Empty)).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<StationAutoCompleteDto>>(entities)).Returns(ValidDtoList());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NoMatchingPattern_ThrowsValidationException()
        {
            var query = new GetStationAutoCompleteQuery { SearchPattern = "ZZZNOMATCH" };
            _mockQueryRepo.Setup(r => r.GetAllStationAsync("ZZZNOMATCH")).ReturnsAsync(new List<UserManagement.Domain.Entities.Station>());

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*No Station found*");
        }
    }
}
