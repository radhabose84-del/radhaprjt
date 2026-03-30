using AutoMapper;
using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using FAM.Application.Common.Interfaces.IAssetGroup;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetGroup.Queries
{
    public sealed class GetAssetGroupQueryHandlerTests
    {
        private readonly Mock<IAssetGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetGroupQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<FAM.Domain.Entities.AssetGroup> { AssetGroupBuilders.ValidEntity() };
            var dtos = new List<AssetGroupDto> { AssetGroupBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetGroupAsync(1, 15, null))
                .ReturnsAsync((entities, 1));
            _mockMapper
                .Setup(m => m.Map<List<AssetGroupDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetGroupQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAssetGroupAsync(1, 15, null))
                .ReturnsAsync((new List<FAM.Domain.Entities.AssetGroup>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<AssetGroupDto>>(It.IsAny<object>()))
                .Returns(new List<AssetGroupDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetGroupQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
