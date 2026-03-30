using AutoMapper;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.Location.Queries.GetSubLocations;
using FAM.Application.SubLocation.Queries.GetSubLocationAutoComplete;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.SubLocation.Queries
{
    public sealed class GetSubLocationAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ISubLocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetSubLocationAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<FAM.Domain.Entities.SubLocation> { SubLocationBuilders.ValidEntity() };
            var dtos = new List<SubLocationAutoCompleteDto> { new SubLocationAutoCompleteDto { Id = 1, SubLocationName = "Test SubLocation" } };

            _mockQueryRepo
                .Setup(r => r.GetSubLocation(It.IsAny<string>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<SubLocationAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetSubLocationAutoCompleteQuery { SearchPattern = "Test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
