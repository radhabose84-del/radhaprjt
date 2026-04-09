using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetRecieptDtlPending;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferReceipt.Queries
{
    public sealed class GetAssetRecieptDtlPendingQueryHandlerTests
    {
        private readonly Mock<IAssetTransferReceiptQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetAssetRecieptDtlPendingQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockUnitLookup.Object, _mockDeptLookup.Object);

        [Fact]
        public async Task Handle_ExistingTransferId_ReturnsDto()
        {
            var hdrDto = new AssetTrasnferReceiptHdrPendingDto
            {
                AssetTransferId = 1,
                DocDate = DateTimeOffset.UtcNow,
                FromUnitId = 1,
                ToUnitId = 2,
                FromDepartmentId = 1,
                ToDepartmentId = 2
            };

            _mockQueryRepo
                .Setup(r => r.GetAssetTransferByIdAsync(1))
                .ReturnsAsync(hdrDto);

            _mockUnitLookup
                .Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<Contracts.Dtos.Lookups.Users.UnitLookupDto>)new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>());

            _mockDeptLookup
                .Setup(d => d.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<Contracts.Dtos.Lookups.Users.DepartmentLookupDto>)new List<Contracts.Dtos.Lookups.Users.DepartmentLookupDto>());

            var result = await CreateSut().Handle(
                new GetAssetRecieptDtlPendingQuery { AssetTransferId = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.AssetTransferId.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistingTransferId_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetAssetTransferByIdAsync(99))
                .ReturnsAsync((AssetTrasnferReceiptHdrPendingDto?)null);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(
                    new GetAssetRecieptDtlPendingQuery { AssetTransferId = 99 },
                    CancellationToken.None));
        }
    }
}
