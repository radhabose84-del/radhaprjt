using AutoMapper;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesByCategoryId;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSubCategories.Queries
{
    public sealed class GetAssetSubCategoriesByCategoryIdQueryHandlerTests
    {
        private readonly Mock<IAssetSubCategoriesQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetSubCategoriesByCategoryIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCategoryId_ReturnsMappedList()
        {
            var entities = new List<AssetSubCategoriesAutoCompleteDto?> { new() { Id = 1 } };
            var dtos = new List<AssetSubCategoriesAutoCompleteDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetSubcategoriesByAssetCategoryIdAsync(1))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetSubCategoriesAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSubCategoriesByCategoryIdQuery { AssetCategoriesId = 1 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetSubcategoriesByAssetCategoryIdAsync(99))
                .ReturnsAsync((List<AssetSubCategoriesAutoCompleteDto?>?)null!);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetSubCategoriesByCategoryIdQuery { AssetCategoriesId = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_EmptyResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetSubcategoriesByAssetCategoryIdAsync(99))
                .ReturnsAsync(new List<AssetSubCategoriesAutoCompleteDto?>());

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetSubCategoriesByCategoryIdQuery { AssetCategoriesId = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
