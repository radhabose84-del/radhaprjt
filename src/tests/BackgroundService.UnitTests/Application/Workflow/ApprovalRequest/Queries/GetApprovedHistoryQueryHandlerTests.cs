using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovedHistory;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using DomainApprovalRequest = BackgroundService.Domain.Entities.Workflow.ApprovalRequest;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalRequest.Queries
{
    public sealed class GetApprovedHistoryQueryHandlerTests
    {
        private readonly Mock<IApprovalRequestQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILookupRepository> _mockLookupRepo = new(MockBehavior.Loose);

        private GetApprovedHistoryQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockLookupRepo.Object);

        [Fact]
        public async Task Handle_ReturnsHistoryWithApproverNames()
        {
            var entities = new List<DomainApprovalRequest>
            {
                new() { Id = 1 }
            };

            _mockQueryRepo
                .Setup(r => r.GetApprovedHistory("PurchaseOrder", 10))
                .ReturnsAsync(entities);

            var dtos = new List<ApprovedHistoryDto>
            {
                new() { ApproverValue = "5", StepOrder = 1, Status = "Approved" }
            };

            _mockMapper
                .Setup(m => m.Map<List<ApprovedHistoryDto>>(entities))
                .Returns(dtos);

            _mockLookupRepo
                .Setup(r => r.GetUserNamesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string> { { 5, "John Doe" } });

            var sut = CreateSut();
            var query = new GetApprovedHistoryQuery
            {
                WorkflowType = "PurchaseOrder",
                ModuleTransactionId = 10
            };

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].ApproverName.Should().Be("John Doe");
        }

        [Fact]
        public async Task Handle_UnknownUser_SetsUnknownApproverName()
        {
            var entities = new List<DomainApprovalRequest>();

            _mockQueryRepo
                .Setup(r => r.GetApprovedHistory("PurchaseOrder", 10))
                .ReturnsAsync(entities);

            var dtos = new List<ApprovedHistoryDto>
            {
                new() { ApproverValue = "999", StepOrder = 1, Status = "Approved" }
            };

            _mockMapper
                .Setup(m => m.Map<List<ApprovedHistoryDto>>(entities))
                .Returns(dtos);

            _mockLookupRepo
                .Setup(r => r.GetUserNamesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string>());

            var sut = CreateSut();
            var query = new GetApprovedHistoryQuery
            {
                WorkflowType = "PurchaseOrder",
                ModuleTransactionId = 10
            };

            var result = await sut.Handle(query, CancellationToken.None);

            result[0].ApproverName.Should().Be("Unknown User");
        }

        [Fact]
        public async Task Handle_EmptyHistory_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetApprovedHistory("PurchaseOrder", 10))
                .ReturnsAsync(new List<DomainApprovalRequest>());

            _mockMapper
                .Setup(m => m.Map<List<ApprovedHistoryDto>>(
                    It.IsAny<List<DomainApprovalRequest>>()))
                .Returns(new List<ApprovedHistoryDto>());

            _mockLookupRepo
                .Setup(r => r.GetUserNamesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string>());

            var sut = CreateSut();

            var result = await sut.Handle(
                new GetApprovedHistoryQuery { WorkflowType = "PurchaseOrder", ModuleTransactionId = 10 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
