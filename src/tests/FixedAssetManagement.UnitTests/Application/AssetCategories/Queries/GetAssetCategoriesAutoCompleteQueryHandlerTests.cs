using AutoMapper;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using FAM.Application.AssetCategories.Queries.GetAssetCategoriesAutoComplete;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetCategories.Queries
{
    public sealed class GetAssetCategoriesAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAssetCategoriesQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetCategoriesAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<FAM.Domain.Entities.AssetCategories> { AssetCategoriesBuilders.ValidEntity() };
            var dtos = new List<AssetCategoriesAutoCompleteDto> { AssetCategoriesBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetAssetCategories(It.IsAny<string>()))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetCategoriesAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetCategoriesAutoCompleteQuery { SearchPattern = "Test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetAssetCategories(It.IsAny<string>()))
                .ReturnsAsync(new List<FAM.Domain.Entities.AssetCategories>());
            _mockMapper
                .Setup(m => m.Map<List<AssetCategoriesAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<AssetCategoriesAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetCategoriesAutoCompleteQuery { SearchPattern = "NONE" }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
