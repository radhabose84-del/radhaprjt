using AutoMapper;
using Contracts.Common;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchase;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetPurchase.Queries
{
    public sealed class GetAssetPurchaseQueryHandlerTests
    {
        private readonly Mock<IAssetPurchaseQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetPurchaseQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<AssetPurchaseDetails> { AssetPurchaseBuilders.ValidEntity() };
            var dtos = new List<AssetPurchaseDetailsDto> { AssetPurchaseBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllPurchaseDetails(1, 10, null))
                .Returns(Task.FromResult<(List<AssetPurchaseDetails>, int)>((entities, 1)));

            _mockMapper
                .Setup(m => m.Map<List<AssetPurchaseDetailsDto>>(It.IsAny<object>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetAssetPurchaseQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<AssetPurchaseDetails> { AssetPurchaseBuilders.ValidEntity() };
            var dtos = new List<AssetPurchaseDetailsDto> { AssetPurchaseBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllPurchaseDetails(2, 5, "search"))
                .Returns(Task.FromResult<(List<AssetPurchaseDetails>, int)>((entities, 11)));

            _mockMapper
                .Setup(m => m.Map<List<AssetPurchaseDetailsDto>>(It.IsAny<object>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetAssetPurchaseQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllPurchaseDetails(1, 10, null))
                .Returns(Task.FromResult<(List<AssetPurchaseDetails>, int)>((new List<AssetPurchaseDetails>(), 0)));

            _mockMapper
                .Setup(m => m.Map<List<AssetPurchaseDetailsDto>>(It.IsAny<object>()))
                .Returns(new List<AssetPurchaseDetailsDto>());

            var result = await CreateSut().Handle(
                new GetAssetPurchaseQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
