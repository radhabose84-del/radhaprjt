using AutoMapper;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterByIdSplit;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetMasterGeneral.Queries
{
    public sealed class GetAssetMasterByIdSplitQueryHandlerTests
    {
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetMasterByIdSplitQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsSplitDto()
        {
            dynamic assetResult = new System.Dynamic.ExpandoObject();
            assetResult.AssetName = "TestAsset";
            var splitDto = new AssetMasterSplitDto();

            _mockRepo
                .Setup(r => r.GetAssetMasterSplitByIdAsync(1))
                .ReturnsAsync(((dynamic)assetResult, (dynamic?)null, (IEnumerable<dynamic>?)null, (IEnumerable<dynamic>?)null));
            _mockMapper
                .Setup(m => m.Map<AssetMasterSplitDto>(It.IsAny<object>()))
                .Returns(splitDto);
            _mockMapper
                .Setup(m => m.Map<AssetParentDTO>(It.IsAny<object>()))
                .Returns(new AssetParentDTO());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetMasterByIdSplitQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NullAssetResult_ReturnsNull()
        {
            _mockRepo
                .Setup(r => r.GetAssetMasterSplitByIdAsync(99))
                .ReturnsAsync(((dynamic?)null, (dynamic?)null, (IEnumerable<dynamic>?)null, (IEnumerable<dynamic>?)null));
            _mockMapper
                .Setup(m => m.Map<AssetMasterSplitDto>(It.IsAny<object>()))
                .Returns((AssetMasterSplitDto?)null!);

            var result = await CreateSut().Handle(
                new GetAssetMasterByIdSplitQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
