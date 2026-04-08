using AutoMapper;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGRNItem;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetPurchase.Queries
{
    public sealed class GetAssetGrnItemQueryHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IAssetPurchaseQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetAssetGrnItemQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockRepo.Object);

        [Fact]
        public async Task Handle_ValidRequest_ReturnsGrnItemList()
        {
            var entities = new List<AssetGrnItem> { new() };
            var dtos = new List<AssetGrnItemDto> { new() };

            _mockRepo
                .Setup(r => r.GetAssetGrnItem("UNIT1", 1, 100))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetGrnItemDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetGrnItemQuery { OldUnitId = "UNIT1", AssetSourceId = 1, GrnNo = 100 },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetAssetGrnItem("UNIT1", 1, 100))
                .ReturnsAsync(new List<AssetGrnItem>());
            _mockMapper
                .Setup(m => m.Map<List<AssetGrnItemDto>>(It.IsAny<object>()))
                .Returns(new List<AssetGrnItemDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetGrnItemQuery { OldUnitId = "UNIT1", AssetSourceId = 1, GrnNo = 100 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
