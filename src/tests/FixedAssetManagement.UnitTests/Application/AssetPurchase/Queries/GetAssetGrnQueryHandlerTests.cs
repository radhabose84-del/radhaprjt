using AutoMapper;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGRN;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Application.AssetMaster.AssetPurchase.Queries;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetPurchase.Queries
{
    public sealed class GetAssetGrnQueryHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IAssetPurchaseQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetAssetGrnQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockRepo.Object);

        [Fact]
        public async Task Handle_ValidRequest_ReturnsGrnList()
        {
            var entities = new List<AssetGrn> { new() };
            var dtos = new List<GetAssetGrnDto> { new() };

            _mockRepo
                .Setup(r => r.GetAssetGrnNo("UNIT1", 1, "GRN001"))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetAssetGrnDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetGrnQuery { OldUnitId = "UNIT1", AssetSourceId = 1, SearchGrnNo = "GRN001" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetAssetGrnNo("UNIT1", 1, "GRN001"))
                .ReturnsAsync(new List<AssetGrn>());
            _mockMapper
                .Setup(m => m.Map<List<GetAssetGrnDto>>(It.IsAny<object>()))
                .Returns(new List<GetAssetGrnDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetGrnQuery { OldUnitId = "UNIT1", AssetSourceId = 1, SearchGrnNo = "GRN001" },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
