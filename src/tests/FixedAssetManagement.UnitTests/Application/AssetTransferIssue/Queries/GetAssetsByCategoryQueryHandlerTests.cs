using AutoMapper;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssertByCategory;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferIssue.Queries
{
    public sealed class GetAssetsByCategoryQueryHandlerTests
    {
        private readonly Mock<IAssetTransferQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetsByCategoryQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidRequest_ReturnsAssetList()
        {
            var entities = new List<GetAssetMasterDto> { new() };
            var dtos = new List<GetAssetMasterDto> { new() };

            _mockRepo
                .Setup(r => r.GetAssetsByCategoryAsync(1, 2))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetAssetMasterDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetsByCategoryQuery { AssetCategoryId = 1, AssetDepartmentId = 2 },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetAssetsByCategoryAsync(1, 2))
                .ReturnsAsync(new List<GetAssetMasterDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetAssetMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetAssetMasterDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetsByCategoryQuery { AssetCategoryId = 1, AssetDepartmentId = 2 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
