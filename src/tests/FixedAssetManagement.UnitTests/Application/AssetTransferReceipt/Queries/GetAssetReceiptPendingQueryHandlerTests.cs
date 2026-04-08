using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptPending;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferReceipt.Queries
{
    public sealed class GetAssetReceiptPendingQueryHandlerTests
    {
        private readonly Mock<IAssetTransferReceiptQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetAssetReceiptPendingQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockUnitLookup.Object, _mockDeptLookup.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var pendingList = new List<AssetTransferReceiptPendingDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllPendingAssetTransferAsync(1, 10, null, null, null, null))
                .ReturnsAsync((pendingList, 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetTransferReceiptPendingDto>>(It.IsAny<object>()))
                .Returns(pendingList);

            var result = await CreateSut().Handle(
                new GetAssetReceiptPendingQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var pendingList = new List<AssetTransferReceiptPendingDto>
            {
                new AssetTransferReceiptPendingDto { AssetTransferId = 1 }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllPendingAssetTransferAsync(2, 5, null, null, null, null))
                .ReturnsAsync((pendingList, 11));

            _mockMapper
                .Setup(m => m.Map<List<AssetTransferReceiptPendingDto>>(It.IsAny<object>()))
                .Returns(pendingList);

            var result = await CreateSut().Handle(
                new GetAssetReceiptPendingQuery { PageNumber = 2, PageSize = 5 },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllPendingAssetTransferAsync(1, 10, null, null, null, null))
                .ReturnsAsync((new List<AssetTransferReceiptPendingDto>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetTransferReceiptPendingDto>>(It.IsAny<object>()))
                .Returns(new List<AssetTransferReceiptPendingDto>());

            await CreateSut().Handle(
                new GetAssetReceiptPendingQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
