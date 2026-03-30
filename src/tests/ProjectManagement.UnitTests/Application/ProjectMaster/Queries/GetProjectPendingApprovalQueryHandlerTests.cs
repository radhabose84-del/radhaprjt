using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectPendingApprovals;

namespace ProjectManagement.UnitTests.Application.ProjectMaster.Queries
{
    public sealed class GetProjectPendingApprovalQueryHandlerTests
    {
        private readonly Mock<IProjectMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Strict);
        private readonly Mock<IWorkflowLookup> _mockWorkflow = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetProjectPendingApprovalQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMisc.Object, _mockWorkflow.Object,
                _mockUserLookup.Object, _mockDeptLookup.Object, _mockIp.Object, _mockMapper.Object);

        private void SetupIpService(int unitId = 1, int userId = 1)
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(unitId);
            _mockIp.Setup(s => s.GetUserId()).Returns(userId);
        }

        private void SetupMiscMaster(int pendingId = 10)
        {
            _mockMisc
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProjectManagement.Domain.Entities.MiscMaster { Id = pendingId });
        }

        [Fact]
        public async Task Handle_EmptyPendingRows_ReturnsEmptyResult()
        {
            SetupIpService();
            SetupMiscMaster();

            _mockRepo
                .Setup(r => r.GetProjectPendingAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<GetProjectPendingApprovalDto>(), 0));

            var result = await CreateSut().Handle(new GetProjectPendingApprovalQuery(), CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_NullPendingRows_ReturnsEmptyResult()
        {
            SetupIpService();
            SetupMiscMaster();

            _mockRepo
                .Setup(r => r.GetProjectPendingAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(((List<GetProjectPendingApprovalDto>)null!, 0));

            var result = await CreateSut().Handle(new GetProjectPendingApprovalQuery(), CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_RowsWithNoMatchingApprover_ReturnsEmptyResult()
        {
            SetupIpService(unitId: 1, userId: 99);
            SetupMiscMaster();

            var rows = new List<GetProjectPendingApprovalDto>
            {
                new() { Id = 1, ProjectCode = "PRJ001", ProjectName = "Test Project", UnitId = 1 }
            };

            _mockRepo
                .Setup(r => r.GetProjectPendingAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((rows, 1));

            // No approver entry has ApproverValue matching userId=99
            _mockWorkflow
                .Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<ApproverListDto>
                {
                    new() { ModuleTransactionId = 1, ApproverValue = "5", ApprovalRequestId = 10 }
                });

            var result = await CreateSut().Handle(new GetProjectPendingApprovalQuery(), CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_RowsMatchingApprover_ReturnsFilteredResult()
        {
            SetupIpService(unitId: 1, userId: 5);
            SetupMiscMaster();

            var rows = new List<GetProjectPendingApprovalDto>
            {
                new() { Id = 1, ProjectCode = "PRJ001", ProjectName = "Test Project", UnitId = 1 }
            };

            _mockRepo
                .Setup(r => r.GetProjectPendingAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((rows, 1));

            _mockWorkflow
                .Setup(w => w.GetApproverListAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<ApproverListDto>
                {
                    new() { ModuleTransactionId = 1, ApproverValue = "5", ApprovalRequestId = 10 }
                });

            _mockUserLookup
                .Setup(u => u.GetAllUserAsync())
                .ReturnsAsync(new List<UserLookupDto>
                {
                    new() { UserId = 5, UserName = "Test Approver" }
                });

            _mockDeptLookup
                .Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.DepartmentLookupDto>());

            var result = await CreateSut().Handle(new GetProjectPendingApprovalQuery(), CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.Items[0].ProjectCode.Should().Be("PRJ001");
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_CallsMiscMasterByName()
        {
            SetupIpService();
            SetupMiscMaster();

            _mockRepo
                .Setup(r => r.GetProjectPendingAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<GetProjectPendingApprovalDto>(), 0));

            await CreateSut().Handle(new GetProjectPendingApprovalQuery(), CancellationToken.None);

            _mockMisc.Verify(
                r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UsesDefaultPagination_WhenNotProvided()
        {
            SetupIpService();
            SetupMiscMaster();

            _mockRepo
                .Setup(r => r.GetProjectPendingAsync(
                    1, 15, null, null, null, null, null,
                    1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<GetProjectPendingApprovalDto>(), 0));

            await CreateSut().Handle(new GetProjectPendingApprovalQuery(), CancellationToken.None);

            _mockRepo.Verify(
                r => r.GetProjectPendingAsync(1, 15, null, null, null, null, null,
                    1, 10, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
