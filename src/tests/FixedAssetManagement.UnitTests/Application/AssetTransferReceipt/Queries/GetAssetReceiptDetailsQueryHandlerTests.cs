using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetails;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferReceipt.Queries
{
    public sealed class GetAssetReceiptDetailsQueryHandlerTests
    {
        private readonly Mock<IAssetTransferReceiptQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetAssetReceiptDetailsQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockUnitLookup.Object, _mockDeptLookup.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var detailsList = new List<AssetReceiptDetailsDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllAssetReceiptDetails(1, 10, null, null, null))
                .ReturnsAsync((detailsList, 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetReceiptDetailsDto>>(It.IsAny<object>()))
                .Returns(detailsList);

            var result = await CreateSut().Handle(
                new GetAssetReceiptDetailsQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var detailsList = new List<AssetReceiptDetailsDto>
            {
                new AssetReceiptDetailsDto { AssetReceiptId = 1 }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetReceiptDetails(2, 5, "search", null, null))
                .ReturnsAsync((detailsList, 11));

            _mockMapper
                .Setup(m => m.Map<List<AssetReceiptDetailsDto>>(It.IsAny<object>()))
                .Returns(detailsList);

            var result = await CreateSut().Handle(
                new GetAssetReceiptDetailsQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAssetReceiptDetails(1, 10, null, null, null))
                .ReturnsAsync((new List<AssetReceiptDetailsDto>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetReceiptDetailsDto>>(It.IsAny<object>()))
                .Returns(new List<AssetReceiptDetailsDto>());

            await CreateSut().Handle(
                new GetAssetReceiptDetailsQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
