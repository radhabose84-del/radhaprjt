using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAllAssetTransfer;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferIssue.Queries
{
    public sealed class GetAllTransferQueryHandlerTests
    {
        private readonly Mock<IAssetTransferQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetAllTransferQueryHandler CreateSut() => new(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ExistingTransferId_ReturnsDetails()
        {
            var dtos = new List<GetAllTransferDtlDto> { AssetTransferIssueBuilders.ValidDtlDto() };

            _mockQueryRepo
                .Setup(r => r.GetAssetTransferByIDAsync(10))
                .ReturnsAsync(dtos);

            var result = await CreateSut().Handle(
                new GetAllTransferQuery { AssetTransferId = 10 },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].AssetTransferId.Should().Be(10);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetAssetTransferByIDAsync(99))
                .ReturnsAsync((List<GetAllTransferDtlDto>?)null);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(new GetAllTransferQuery { AssetTransferId = 99 }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_EmptyResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetAssetTransferByIDAsync(99))
                .ReturnsAsync(new List<GetAllTransferDtlDto>());

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(new GetAllTransferQuery { AssetTransferId = 99 }, CancellationToken.None));
        }
    }
}
