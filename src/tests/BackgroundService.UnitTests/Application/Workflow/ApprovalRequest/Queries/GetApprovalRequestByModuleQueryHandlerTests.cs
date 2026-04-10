using AutoMapper;
using BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestById;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using Contracts.Interfaces;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalRequest.Queries
{
    public sealed class GetApprovalRequestByModuleQueryHandlerTests
    {
        private readonly Mock<IApprovalRequestQuery> _mockRepository = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private GetApprovalRequestByModuleQueryHandler CreateSut() =>
            new(_mockRepository.Object, _mockMapper.Object, _mockIp.Object, _mockRepository.Object);

        [Fact]
        public async Task Handle_NoResults_ReturnsEmptyList()
        {
            _mockIp.Setup(ip => ip.GetUserId()).Returns(1);

            _mockRepository
                .Setup(r => r.GetByModuleAsync(10, 1))
                .ReturnsAsync(new List<ApprovalRequestWithLinesDto>());

            var sut = CreateSut();
            var query = new GetApprovalRequestByModuleQuery
            {
                ModuleTransactionId = 10,
                WorkflowTypeId = 1
            };

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NullResults_ReturnsEmptyList()
        {
            _mockIp.Setup(ip => ip.GetUserId()).Returns(1);

            _mockRepository
                .Setup(r => r.GetByModuleAsync(10, 1))
                .ReturnsAsync((List<ApprovalRequestWithLinesDto>?)null!);

            var sut = CreateSut();
            var query = new GetApprovalRequestByModuleQuery
            {
                ModuleTransactionId = 10,
                WorkflowTypeId = 1
            };

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_FiltersResultsByCurrentUser()
        {
            _mockIp.Setup(ip => ip.GetUserId()).Returns(5);

            var data = new List<ApprovalRequestWithLinesDto>
            {
                new() { Id = 1, ApproverValue = "5", ModuleTransactionId = 10, WorkflowTypeId = 1 },
                new() { Id = 2, ApproverValue = "99", ModuleTransactionId = 10, WorkflowTypeId = 1 }
            };

            _mockRepository
                .Setup(r => r.GetByModuleAsync(10, 1))
                .ReturnsAsync(data);

            var sut = CreateSut();
            var query = new GetApprovalRequestByModuleQuery
            {
                ModuleTransactionId = 10,
                WorkflowTypeId = 1
            };

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(1);
        }
    }
}
