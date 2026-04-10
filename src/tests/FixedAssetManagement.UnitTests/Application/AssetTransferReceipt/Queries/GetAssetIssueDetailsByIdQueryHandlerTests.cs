using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetIssueDetailsById;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferReceipt.Queries
{
    public sealed class GetAssetIssueDetailsByIdQueryHandlerTests
    {
        private readonly Mock<IAssetTransferReceiptQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetAssetIssueDetailsByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ThrowsNotImplementedException()
        {
            // Handler currently throws NotImplementedException
            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetIssueDetailsByIdQuery { AssetTransferId = 1 }, CancellationToken.None);

            await act.Should().ThrowAsync<NotImplementedException>();
        }
    }
}
