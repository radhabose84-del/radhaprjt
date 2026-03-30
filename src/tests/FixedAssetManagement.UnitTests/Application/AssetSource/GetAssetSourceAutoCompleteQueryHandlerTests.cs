using AutoMapper;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSource
{
    public sealed class GetAssetSourceAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAssetPurchaseQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetSourceAutoCompleteQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ValidPattern_ReturnsDtoList()
        {
            var entities = new List<FAM.Domain.Entities.AssetSource>
            {
                AssetSourceBuilders.ValidEntity()
            };
            var dtoList = new List<AssetSourceAutoCompleteDto>
            {
                AssetSourceBuilders.ValidAutoCompleteDto()
            };

            _mockQueryRepo
                .Setup(r => r.GetAssetSources(It.IsAny<string>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<AssetSourceAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSourceAutoCompleteQuery { SearchPattern = "SRC" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ValidPattern_CallsRepositoryOnce()
        {
            var entities = new List<FAM.Domain.Entities.AssetSource>();
            var dtoList = new List<AssetSourceAutoCompleteDto>();

            _mockQueryRepo
                .Setup(r => r.GetAssetSources(It.IsAny<string>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<AssetSourceAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetAssetSourceAutoCompleteQuery { SearchPattern = "SRC" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAssetSources(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidPattern_PublishesAuditEvent()
        {
            var entities = new List<FAM.Domain.Entities.AssetSource>();
            var dtoList = new List<AssetSourceAutoCompleteDto>();

            _mockQueryRepo
                .Setup(r => r.GetAssetSources(It.IsAny<string>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<AssetSourceAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetAssetSourceAutoCompleteQuery { SearchPattern = "SRC" },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
