using AutoMapper;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetDtlToTransfer;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetBulkAssetToTransfer;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferIssue.Queries
{
    public sealed class GetBulkAssetToTransferQueryHandlerTests
    {
        private readonly Mock<IAssetTransferQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetBulkAssetToTransferQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccess()
        {
            var entities = new List<GetAssetDetailsToTransferHdrDto> { new() };
            var dtos = new List<GetAssetDetailsToTransferHdrDto> { new() };

            _mockRepo
                .Setup(r => r.GetAssetDetailsToTransferByFiltersAsync("CUST1", 1, ""))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetAssetDetailsToTransferHdrDto>>(It.IsAny<object>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetBulkAssetToTransferQuery { CustodianId = "CUST1", DepartmentId = 1, CategoryID = "" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyCustodianId_ReturnsFailure()
        {
            var result = await CreateSut().Handle(
                new GetBulkAssetToTransferQuery { CustodianId = "", DepartmentId = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("CustodianId is required");
        }

        [Fact]
        public async Task Handle_NullCustodianId_ReturnsFailure()
        {
            var result = await CreateSut().Handle(
                new GetBulkAssetToTransferQuery { CustodianId = null, DepartmentId = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
