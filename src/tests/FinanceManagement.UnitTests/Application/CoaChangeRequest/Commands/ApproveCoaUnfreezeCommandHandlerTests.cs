using Contracts.Events.Coa;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Application.CoaChangeRequest.Commands.ApproveCoaUnfreeze;
using FinanceManagement.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FinanceManagement.UnitTests.Application.CoaChangeRequest.Commands
{
    public sealed class ApproveCoaUnfreezeCommandHandlerTests
    {
        private const int CfoRoleId = 15;
        private const int SysAdminRoleId = 20;

        private readonly Mock<ICoaChangeRequestCommandRepository> _mockCmd = new(MockBehavior.Loose);
        private readonly Mock<ICoaFreezeCommandRepository> _mockFreeze = new(MockBehavior.Loose);
        private readonly Mock<IRoleUserLookup> _mockRoles = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private ApproveCoaUnfreezeCommandHandler CreateSut()
        {
            var options = Options.Create(new CoaUnfreezeOptions
            {
                CfoRoleId = CfoRoleId,
                SystemAdminRoleId = SysAdminRoleId,
                FcRoleId = 21,
                InternalAuditRoleId = 22,
                DefaultWindowMinutes = 60
            });
            return new ApproveCoaUnfreezeCommandHandler(
                _mockCmd.Object, _mockFreeze.Object, _mockRoles.Object, _mockOutbox.Object,
                _mockIp.Object, _mockTz.Object, options, _mockMediator.Object);
        }

        private void SetupSession(int userId, int companyId = 1)
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(companyId);
            _mockIp.Setup(x => x.GetUserId()).Returns(userId);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(new DateTimeOffset(2026, 6, 22, 10, 0, 0, TimeSpan.Zero));
        }

        // AC1 — the same person cannot give both approvals.
        [Fact]
        public async Task Handle_SamePersonBothApprovals_IsBlockedWithDualApprovalMessage()
        {
            const int userId = 7;
            SetupSession(userId);
            // SysAdmin slot already filled by user 7; the same user (also CFO) now tries the CFO slot.
            _mockCmd.Setup(r => r.GetUnfreezeRequestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CoaUnfreezeRequest
                {
                    Id = 100,
                    CompanyId = 1,
                    RequestStatus = CoaUnfreezeRequestStatus.PendingApproval,
                    SysAdminApproverUserId = userId,
                    WindowMinutes = 60
                });
            _mockRoles.Setup(r => r.UserHasRoleAsync(userId, CfoRoleId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockRoles.Setup(r => r.UserHasRoleAsync(userId, SysAdminRoleId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var act = async () => await CreateSut().Handle(new ApproveCoaUnfreezeCommand { UnfreezeRequestId = 100 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("Dual approval required — approvers must be different users.");
            _mockFreeze.Verify(f => f.OpenUnfreezeWindowAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // AC2 — two distinct approvers (CFO + System Admin) open the window and fire alerts.
        [Fact]
        public async Task Handle_SecondDistinctApprover_OpensWindowAndPublishesAlert()
        {
            const int sysAdminUser = 9;
            SetupSession(sysAdminUser);
            // CFO slot already filled by user 7; a distinct System Admin (user 9) now approves.
            var req = new CoaUnfreezeRequest
            {
                Id = 100,
                CompanyId = 1,
                RequestStatus = CoaUnfreezeRequestStatus.PendingApproval,
                CfoApproverUserId = 7,
                WindowMinutes = 60
            };
            _mockCmd.Setup(r => r.GetUnfreezeRequestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(req);
            _mockRoles.Setup(r => r.UserHasRoleAsync(sysAdminUser, CfoRoleId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockRoles.Setup(r => r.UserHasRoleAsync(sysAdminUser, SysAdminRoleId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockRoles.Setup(r => r.GetEmailsByRoleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "cfo@x.com" });

            var result = await CreateSut().Handle(new ApproveCoaUnfreezeCommand { UnfreezeRequestId = 100 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            req.RequestStatus.Should().Be(CoaUnfreezeRequestStatus.WindowOpen);
            req.SysAdminApproverUserId.Should().Be(sysAdminUser);
            _mockFreeze.Verify(f => f.OpenUnfreezeWindowAsync(1, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockOutbox.Verify(o => o.ScheduleWithoutSaveAsync(It.IsAny<CoaUnfreezeAlertEvent>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_FirstApprover_RecordsApprovalWithoutOpeningWindow()
        {
            const int cfoUser = 7;
            SetupSession(cfoUser);
            var req = new CoaUnfreezeRequest
            {
                Id = 100,
                CompanyId = 1,
                RequestStatus = CoaUnfreezeRequestStatus.PendingApproval,
                WindowMinutes = 60
            };
            _mockCmd.Setup(r => r.GetUnfreezeRequestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(req);
            _mockRoles.Setup(r => r.UserHasRoleAsync(cfoUser, CfoRoleId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockRoles.Setup(r => r.UserHasRoleAsync(cfoUser, SysAdminRoleId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await CreateSut().Handle(new ApproveCoaUnfreezeCommand { UnfreezeRequestId = 100 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            req.CfoApproverUserId.Should().Be(cfoUser);
            req.RequestStatus.Should().Be(CoaUnfreezeRequestStatus.PendingApproval);
            _mockFreeze.Verify(f => f.OpenUnfreezeWindowAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CallerHoldsNeitherRole_IsBlocked()
        {
            const int userId = 5;
            SetupSession(userId);
            _mockCmd.Setup(r => r.GetUnfreezeRequestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CoaUnfreezeRequest { Id = 100, CompanyId = 1, RequestStatus = CoaUnfreezeRequestStatus.PendingApproval, WindowMinutes = 60 });
            _mockRoles.Setup(r => r.UserHasRoleAsync(userId, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var act = async () => await CreateSut().Handle(new ApproveCoaUnfreezeCommand { UnfreezeRequestId = 100 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*CFO or System Admin*");
        }
    }
}
