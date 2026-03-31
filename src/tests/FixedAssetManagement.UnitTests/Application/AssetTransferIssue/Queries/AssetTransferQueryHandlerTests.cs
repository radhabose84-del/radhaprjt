using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetTransfer.Queries.GetAssetTransfered;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferIssue.Queries
{
    public sealed class AssetTransferQueryHandlerTests
    {
        private readonly Mock<IAssetTransferQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private AssetTransferQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockUnitLookup.Object, _mockDeptLookup.Object);

        private void SetupLookupMocks()
        {
            _mockUnitLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitLookupDto>());
            _mockDeptLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DepartmentLookupDto>());
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtos = new List<AssetTransferDto> { AssetTransferIssueBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null, null))
                .ReturnsAsync((dtos, 1));

            _mockMapper
                .Setup(m => m.Map<List<AssetTransferDto>>(dtos))
                .Returns(dtos);

            SetupLookupMocks();

            var result = await CreateSut().Handle(
                new AssetTransferQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtos = new List<AssetTransferDto> { AssetTransferIssueBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "search", null, null))
                .ReturnsAsync((dtos, 11));

            _mockMapper
                .Setup(m => m.Map<List<AssetTransferDto>>(dtos))
                .Returns(dtos);

            SetupLookupMocks();

            var result = await CreateSut().Handle(
                new AssetTransferQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyDtos = new List<AssetTransferDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null, null))
                .ReturnsAsync((emptyDtos, 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetTransferDto>>(emptyDtos))
                .Returns(emptyDtos);

            var result = await CreateSut().Handle(
                new AssetTransferQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
