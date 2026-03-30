using AutoMapper;
using Contracts.Common;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmc;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetAmc.Queries
{
    public sealed class GetAssetAmcQueryHandlerTests
    {
        private readonly Mock<IAssetAmcQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetAmcQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<FAM.Domain.Entities.AssetMaster.AssetAmc>
            {
                AssetAmcBuilders.ValidEntity()
            };
            var dtoList = new List<AssetAmcDto> { AssetAmcBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetAmcAsync(1, 15, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<AssetAmcDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetAmcQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<FAM.Domain.Entities.AssetMaster.AssetAmc>();
            var dtoList = new List<AssetAmcDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllAssetAmcAsync(3, 10, "vendor"))
                .ReturnsAsync((entities, 25));

            _mockMapper
                .Setup(m => m.Map<List<AssetAmcDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetAmcQuery { PageNumber = 3, PageSize = 10, SearchTerm = "vendor" },
                CancellationToken.None);

            result.PageNumber.Should().Be(3);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(25);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var entities = new List<FAM.Domain.Entities.AssetMaster.AssetAmc>();
            var dtoList = new List<AssetAmcDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllAssetAmcAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((entities, 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetAmcDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetAssetAmcQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
