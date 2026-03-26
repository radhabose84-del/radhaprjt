using AutoMapper;
using FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCost;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetAdditionalCost.Queries
{
    public sealed class GetAssetAdditionalCostQueryHandlerTests
    {
        private readonly Mock<IAssetAdditionalCostQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetAdditionalCostQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entity = AssetAdditionalCostBuilders.ValidEntity();
            var dto = AssetAdditionalCostBuilders.ValidDto();
            var entityList = new List<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost> { entity };

            _mockQueryRepo
                .Setup(r => r.GetAllAdditionalCostGroupAsync(1, 10, null))
                .ReturnsAsync((entityList, 1));

            _mockMapper
                .Setup(m => m.Map<List<AssetAdditionalCostDto>>(It.IsAny<object>()))
                .Returns(new List<AssetAdditionalCostDto> { dto });

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetAdditionalCostQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entityList = new List<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>();

            _mockQueryRepo
                .Setup(r => r.GetAllAdditionalCostGroupAsync(2, 5, "search"))
                .ReturnsAsync((entityList, 11));

            _mockMapper
                .Setup(m => m.Map<List<AssetAdditionalCostDto>>(It.IsAny<object>()))
                .Returns(new List<AssetAdditionalCostDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetAdditionalCostQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAdditionalCostGroupAsync(1, 10, null))
                .ReturnsAsync((new List<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetAdditionalCostDto>>(It.IsAny<object>()))
                .Returns(new List<AssetAdditionalCostDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetAdditionalCostQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
