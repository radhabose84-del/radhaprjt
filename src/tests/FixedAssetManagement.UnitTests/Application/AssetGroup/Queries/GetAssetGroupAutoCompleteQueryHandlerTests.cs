using AutoMapper;
using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using FAM.Application.AssetGroup.Queries.GetAssetGroupAutoComplete;
using FAM.Application.Common.Interfaces.IAssetGroup;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetGroup.Queries
{
    public sealed class GetAssetGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAssetGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetGroupAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<FAM.Domain.Entities.AssetGroup> { AssetGroupBuilders.ValidEntity() };
            var dtos = new List<AssetGroupAutoCompleteDTO> { AssetGroupBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetAssetGroups(It.IsAny<string>()))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetGroupAutoCompleteDTO>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetGroupAutoCompleteQuery { SearchPattern = "AG" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetAssetGroups(It.IsAny<string>()))
                .ReturnsAsync(new List<FAM.Domain.Entities.AssetGroup>());
            _mockMapper
                .Setup(m => m.Map<List<AssetGroupAutoCompleteDTO>>(It.IsAny<object>()))
                .Returns(new List<AssetGroupAutoCompleteDTO>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetGroupAutoCompleteQuery { SearchPattern = "NONE" }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
