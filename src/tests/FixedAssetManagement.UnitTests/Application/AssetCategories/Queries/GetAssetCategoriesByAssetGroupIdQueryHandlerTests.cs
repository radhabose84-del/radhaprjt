using AutoMapper;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using FAM.Application.AssetCategories.Queries.GetAssetCategoriesByAssetGroupId;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetCategories.Queries
{
    public sealed class GetAssetCategoriesByAssetGroupIdQueryHandlerTests
    {
        private readonly Mock<IAssetCategoriesQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetCategoriesByAssetGroupIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidGroupId_ReturnsMappedList()
        {
            var entities = new List<AssetCategoriesAutoCompleteDto?> { new() { Id = 1 } };
            var dtos = new List<AssetCategoriesAutoCompleteDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetByAssetgroupIdAsync(1))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetCategoriesAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetCategoriesByAssetGroupIdQuery { AssetGroupId = 1 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByAssetgroupIdAsync(99))
                .ReturnsAsync((List<AssetCategoriesAutoCompleteDto?>?)null!);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetCategoriesByAssetGroupIdQuery { AssetGroupId = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_EmptyResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByAssetgroupIdAsync(99))
                .ReturnsAsync(new List<AssetCategoriesAutoCompleteDto?>());

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetCategoriesByAssetGroupIdQuery { AssetGroupId = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
