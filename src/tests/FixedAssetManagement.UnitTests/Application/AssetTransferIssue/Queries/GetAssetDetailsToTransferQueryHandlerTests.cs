using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetDtlToTransfer;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferIssue.Queries
{
    public sealed class GetAssetDetailsToTransferQueryHandlerTests
    {
        private readonly Mock<IAssetTransferQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetAssetDetailsToTransferQueryHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ValidAssetId_ReturnsDto()
        {
            var dto = new GetAssetDetailsToTransferHdrDto();

            _mockRepo
                .Setup(r => r.GetAssetDetailsToTransferByIdAsync(1))
                .ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetAssetDetailsToTransferQuery { AssetId = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockRepo
                .Setup(r => r.GetAssetDetailsToTransferByIdAsync(99))
                .ReturnsAsync((GetAssetDetailsToTransferHdrDto?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetDetailsToTransferQuery { AssetId = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
