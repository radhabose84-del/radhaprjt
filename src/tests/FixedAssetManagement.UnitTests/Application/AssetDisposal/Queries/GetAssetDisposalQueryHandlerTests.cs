using AutoMapper;
using Contracts.Common;
using FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposal;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetDisposal;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetDisposal.Queries
{
    public sealed class GetAssetDisposalQueryHandlerTests
    {
        private readonly Mock<IAssetDisposalQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetDisposalQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<FAM.Domain.Entities.AssetMaster.AssetDisposal>
            {
                AssetDisposalBuilders.ValidEntity()
            };
            var dtoList = new List<AssetDisposalDto> { AssetDisposalBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetDisposalAsync(1, 15, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<AssetDisposalDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetDisposalQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<FAM.Domain.Entities.AssetMaster.AssetDisposal>();
            var dtoList = new List<AssetDisposalDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllAssetDisposalAsync(2, 5, null))
                .ReturnsAsync((entities, 10));

            _mockMapper
                .Setup(m => m.Map<List<AssetDisposalDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetDisposalQuery { PageNumber = 2, PageSize = 5 },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(10);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsSuccess()
        {
            var entities = new List<FAM.Domain.Entities.AssetMaster.AssetDisposal>();
            var dtoList = new List<AssetDisposalDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllAssetDisposalAsync(1, 15, null))
                .ReturnsAsync((entities, 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetDisposalDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetDisposalQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
