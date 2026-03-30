using AutoMapper;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesAutoComplete;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSubCategories.Queries
{
    public sealed class GetAssetSubCategoriesAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAssetSubCategoriesQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetSubCategoriesAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<FAM.Domain.Entities.AssetSubCategories> { AssetSubCategoriesBuilders.ValidEntity() };
            var dtos = new List<AssetSubCategoriesAutoCompleteDto> { AssetSubCategoriesBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetAssetSubCategories(It.IsAny<string>()))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetSubCategoriesAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSubCategoriesAutoCompleteQuery { SearchPattern = "Test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetAssetSubCategories(It.IsAny<string>()))
                .ReturnsAsync(new List<FAM.Domain.Entities.AssetSubCategories>());
            _mockMapper
                .Setup(m => m.Map<List<AssetSubCategoriesAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<AssetSubCategoriesAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSubCategoriesAutoCompleteQuery { SearchPattern = "NONE" }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
