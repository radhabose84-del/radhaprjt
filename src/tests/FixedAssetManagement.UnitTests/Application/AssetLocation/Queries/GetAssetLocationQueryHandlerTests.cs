using AutoMapper;
using Contracts.Common;
using FAM.Application.AssetLocation.Queries.GetAssetLocation;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetLocation.Queries
{
    public sealed class GetAssetLocationQueryHandlerTests
    {
        private readonly Mock<IAssetLocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetLocationQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var locations = new List<FAM.Domain.Entities.AssetMaster.AssetLocation>
            {
                AssetLocationTestBuilders.ValidEntity(1)
            };
            var dtos = new List<AssetLocationDto> { AssetLocationTestBuilders.ValidDto(1) };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetLocationAsync(1, 10, null))
                .ReturnsAsync((locations, 1));

            _mockMapper
                .Setup(m => m.Map<List<AssetLocationDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetLocationQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var locations = new List<FAM.Domain.Entities.AssetMaster.AssetLocation>
            {
                AssetLocationTestBuilders.ValidEntity(1)
            };
            var dtos = new List<AssetLocationDto> { AssetLocationTestBuilders.ValidDto(1) };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetLocationAsync(2, 5, "search"))
                .ReturnsAsync((locations, 11));

            _mockMapper
                .Setup(m => m.Map<List<AssetLocationDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetLocationQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAssetLocationAsync(1, 10, null))
                .ReturnsAsync((new List<FAM.Domain.Entities.AssetMaster.AssetLocation>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetLocationDto>>(It.IsAny<object>()))
                .Returns(new List<AssetLocationDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetLocationQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAssetLocationAsync(1, 10, null))
                .ReturnsAsync((new List<FAM.Domain.Entities.AssetMaster.AssetLocation>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetLocationDto>>(It.IsAny<object>()))
                .Returns(new List<AssetLocationDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetAssetLocationQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
