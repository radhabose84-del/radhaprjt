using AutoMapper;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSubCategories.Queries
{
    public sealed class GetAssetSubCategoriesQueryHandlerTests
    {
        private readonly Mock<IAssetSubCategoriesQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetSubCategoriesQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtos = new List<AssetSubCategoriesDto> { AssetSubCategoriesBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetSubCategoriesAsync(1, 15, null))
                .ReturnsAsync((dtos, 1));
            _mockMapper
                .Setup(m => m.Map<List<AssetSubCategoriesDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSubCategoriesQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAssetSubCategoriesAsync(1, 15, null))
                .ReturnsAsync((new List<AssetSubCategoriesDto>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<AssetSubCategoriesDto>>(It.IsAny<object>()))
                .Returns(new List<AssetSubCategoriesDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSubCategoriesQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
