using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.PeriodStatusOverride.Commands.ApprovePeriodReversal;
using FinanceManagement.Application.PeriodStatusOverride.Dto;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.PeriodStatusOverride.Commands
{
    /// <summary>
    /// US-GL03-02 / AC#3 — dual-approval (CFO + SysAdmin) → auto-apply path is security-critical.
    /// </summary>
    public sealed class ApprovePeriodReversalCommandHandlerTests
    {
        private readonly Mock<IPeriodStatusOverrideCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPeriodStatusOverrideQueryRepository>   _mockQueryRepo   = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService>  _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMediator>         _mockMediator = new(MockBehavior.Loose);

        private ApprovePeriodReversalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockTz.Object, _mockMediator.Object);

        private void SetupSession(int userId = 99)
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockIp.Setup(x => x.GetUserId()).Returns(userId);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
        }

        // ─── CFO only → stays PENDING, no auto-apply ────────────────────────

        [Fact]
        public async Task Handle_CfoOnly_DoesNotAutoApplyPeriodStatusChange()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
                PeriodStatusOverrideBuilders.PendingOverrideDto(cfoApproverId: null, sysAdminApproverId: null));
            _mockCommandRepo.Setup(r => r.UpdateAsync(
                It.IsAny<FinanceManagement.Domain.Entities.PeriodStatusOverride>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await CreateSut().Handle(new ApprovePeriodReversalCommand { OverrideId = 1, Role = "CFO" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("Awaiting");

            _mockCommandRepo.Verify(r => r.ApplyPeriodStatusChangeAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_SysAdminAfterCfo_TriggersAutoApply()
        {
            SetupSession(userId: 99);
            // CFO already recorded (user 50), now SysAdmin (user 99) approves
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
                PeriodStatusOverrideBuilders.PendingOverrideDto(
                    cfoApproverId: 50,
                    sysAdminApproverId: null));
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("PSO", "APPLIED")).ReturnsAsync(500);
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.HardClosedPeriodSnapshot());

            _mockCommandRepo.Setup(r => r.UpdateAsync(
                It.IsAny<FinanceManagement.Domain.Entities.PeriodStatusOverride>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockCommandRepo.Setup(r => r.ApplyPeriodStatusChangeAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(
                new ApprovePeriodReversalCommand { OverrideId = 1, Role = "SysAdmin" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("flipped");

            _mockCommandRepo.Verify(r => r.ApplyPeriodStatusChangeAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_BothApproved_PublishesReversalDomainEvent()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
                PeriodStatusOverrideBuilders.PendingOverrideDto(cfoApproverId: 50));
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("PSO", "APPLIED")).ReturnsAsync(500);
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.HardClosedPeriodSnapshot());
            _mockCommandRepo.Setup(r => r.UpdateAsync(
                It.IsAny<FinanceManagement.Domain.Entities.PeriodStatusOverride>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockCommandRepo.Setup(r => r.ApplyPeriodStatusChangeAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(new ApprovePeriodReversalCommand { OverrideId = 1, Role = "SysAdmin" }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<PeriodStatusChangedDomainEvent>(e => e.IsReversal && e.OverrideId == 1),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ─── Segregation-of-duties (SoD) ────────────────────────────────────

        [Fact]
        public async Task Handle_RequesterTriesToApprove_Throws()
        {
            SetupSession(userId: 42);   // same as RequestedBy in builder
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
                PeriodStatusOverrideBuilders.PendingOverrideDto(requestedBy: 42));

            Func<Task> act = () => CreateSut().Handle(
                new ApprovePeriodReversalCommand { OverrideId = 1, Role = "CFO" }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*self-approve*");
        }

        [Fact]
        public async Task Handle_CfoAlreadyApproved_AndCfoApprovesAgain_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
                PeriodStatusOverrideBuilders.PendingOverrideDto(cfoApproverId: 50));

            Func<Task> act = () => CreateSut().Handle(
                new ApprovePeriodReversalCommand { OverrideId = 1, Role = "CFO" }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*CFO approval already*");
        }

        [Fact]
        public async Task Handle_InvalidRole_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
                PeriodStatusOverrideBuilders.PendingOverrideDto());

            Func<Task> act = () => CreateSut().Handle(
                new ApprovePeriodReversalCommand { OverrideId = 1, Role = "FinanceManager" }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*CFO*SysAdmin*");
        }

        [Fact]
        public async Task Handle_OverrideAlreadyApplied_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
                PeriodStatusOverrideBuilders.PendingOverrideDto(statusCode: "APPLIED"));

            Func<Task> act = () => CreateSut().Handle(
                new ApprovePeriodReversalCommand { OverrideId = 1, Role = "CFO" }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*finalised*");
        }

        [Fact]
        public async Task Handle_OverrideNotFound_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PeriodStatusOverrideDto?)null);

            Func<Task> act = () => CreateSut().Handle(
                new ApprovePeriodReversalCommand { OverrideId = 99, Role = "CFO" }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }
    }
}
