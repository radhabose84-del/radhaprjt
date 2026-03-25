using AutoMapper;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Location.Queries.GetLocationAutoComplete;
using FAM.Application.Location.Queries.GetLocations;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.Location.Queries
{
    public sealed class GetLocationAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ILocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetLocationAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<FAM.Domain.Entities.Location> { LocationBuilders.ValidEntity() };
            var dtos = new List<LocationAutoCompleteDto> { new LocationAutoCompleteDto { Id = 1, LocationName = "Test Location" } };

            _mockQueryRepo
                .Setup(r => r.GetLocation(It.IsAny<string>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<LocationAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetLocationAutoCompleteQuery { SearchPattern = "Test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
