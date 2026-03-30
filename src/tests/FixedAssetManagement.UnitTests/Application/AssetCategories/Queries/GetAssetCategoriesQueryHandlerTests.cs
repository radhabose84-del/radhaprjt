using AutoMapper;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetCategories.Queries
{
    public sealed class GetAssetCategoriesQueryHandlerTests
    {
        private readonly Mock<IAssetCategoriesQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetCategoriesQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtos = new List<AssetCategoriesDto> { AssetCategoriesBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetCategoriesAsync(1, 15, null))
                .ReturnsAsync((dtos, 1));
            _mockMapper
                .Setup(m => m.Map<List<AssetCategoriesDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetCategoriesQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAssetCategoriesAsync(1, 15, null))
                .ReturnsAsync((new List<AssetCategoriesDto>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<AssetCategoriesDto>>(It.IsAny<object>()))
                .Returns(new List<AssetCategoriesDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetCategoriesQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
