using AutoMapper;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetPurchase.Queries
{
    public sealed class GetAssetSourceAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IAssetPurchaseQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetAssetSourceAutoCompleteQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockRepo.Object);

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSourceList()
        {
            var entities = new List<FAM.Domain.Entities.AssetSource> { new() };
            var dtos = new List<AssetSourceAutoCompleteDto> { new() };

            _mockRepo
                .Setup(r => r.GetAssetSources("test"))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetSourceAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSourceAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetAssetSources(""))
                .ReturnsAsync(new List<FAM.Domain.Entities.AssetSource>());
            _mockMapper
                .Setup(m => m.Map<List<AssetSourceAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<AssetSourceAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSourceAutoCompleteQuery { SearchPattern = null }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
