using AutoMapper;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroupAutoComplete;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSubGroup.Queries
{
    public sealed class GetAssetSubGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAssetSubGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetSubGroupAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<FAM.Domain.Entities.AssetSubGroup> { AssetSubGroupBuilders.ValidEntity() };
            var dtos = new List<AssetSubGroupAutoCompleteDTO> { AssetSubGroupBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetAssetSubGroups(It.IsAny<string>()))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetSubGroupAutoCompleteDTO>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSubGroupAutoCompleteQuery { SearchPattern = "SG" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetAssetSubGroups(It.IsAny<string>()))
                .ReturnsAsync(new List<FAM.Domain.Entities.AssetSubGroup>());
            _mockMapper
                .Setup(m => m.Map<List<AssetSubGroupAutoCompleteDTO>>(It.IsAny<object>()))
                .Returns(new List<AssetSubGroupAutoCompleteDTO>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSubGroupAutoCompleteQuery { SearchPattern = "NONE" }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
