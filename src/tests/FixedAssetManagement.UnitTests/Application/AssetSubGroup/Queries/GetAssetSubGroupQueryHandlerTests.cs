using AutoMapper;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSubGroup.Queries
{
    public sealed class GetAssetSubGroupQueryHandlerTests
    {
        private readonly Mock<IAssetSubGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetSubGroupQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<FAM.Domain.Entities.AssetSubGroup> { AssetSubGroupBuilders.ValidEntity() };
            var dtos = new List<AssetSubGroupDto> { AssetSubGroupBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetSubGroupAsync(1, 15, null))
                .ReturnsAsync((entities, 1));
            _mockMapper
                .Setup(m => m.Map<List<AssetSubGroupDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSubGroupQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAssetSubGroupAsync(1, 15, null))
                .ReturnsAsync((new List<FAM.Domain.Entities.AssetSubGroup>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<AssetSubGroupDto>>(It.IsAny<object>()))
                .Returns(new List<AssetSubGroupDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSubGroupQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
