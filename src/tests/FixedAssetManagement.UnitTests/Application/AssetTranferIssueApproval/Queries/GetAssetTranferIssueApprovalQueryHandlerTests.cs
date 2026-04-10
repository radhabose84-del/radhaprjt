using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueApproval;
using FAM.Application.Common.Interfaces.IAssetTransferIssueApproval;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTranferIssueApproval.Queries
{
    public sealed class GetAssetTranferIssueApprovalQueryHandlerTests
    {
        private readonly Mock<IAssetTransferIssueApprovalQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Strict);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Strict);

        private GetAssetTranferIssueApprovalQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockUnitLookup.Object, _mockDeptLookup.Object);

        private void SetupLookupMocks()
        {
            _mockUnitLookup
                .Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitLookupDto>());
            _mockDeptLookup
                .Setup(d => d.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DepartmentLookupDto>());
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<AssetTransferIssueApprovalDto> { new() { FromUnitId = 1, ToUnitId = 2, FromDepartmentId = 1, ToDepartmentId = 2 } };

            _mockRepo
                .Setup(r => r.GetAllPendingAssetTransferAsync(1, 15, null, null, null))
                .ReturnsAsync((entities, 1));
            _mockMapper
                .Setup(m => m.Map<List<AssetTransferIssueApprovalDto>>(It.IsAny<object>()))
                .Returns(entities);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            SetupLookupMocks();

            var result = await CreateSut().Handle(
                new GetAssetTranferIssueApprovalQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockRepo
                .Setup(r => r.GetAllPendingAssetTransferAsync(1, 15, null, null, null))
                .ReturnsAsync((new List<AssetTransferIssueApprovalDto>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<AssetTransferIssueApprovalDto>>(It.IsAny<object>()))
                .Returns(new List<AssetTransferIssueApprovalDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            SetupLookupMocks();

            var result = await CreateSut().Handle(
                new GetAssetTranferIssueApprovalQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
