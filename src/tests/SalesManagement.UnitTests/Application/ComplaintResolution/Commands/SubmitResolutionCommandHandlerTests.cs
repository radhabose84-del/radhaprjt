using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Events.Notifications;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.ComplaintResolution.Commands.SubmitResolution;
using SalesManagement.Application.MiscMaster.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintResolution.Commands;

public sealed class SubmitResolutionCommandHandlerTests
{
    private readonly Mock<IComplaintResolutionCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IComplaintResolutionQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ITimeZoneService> _mockTzService = new(MockBehavior.Loose);
    private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IComplaintQueryRepository> _mockComplaintQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IOfficerAgentUserLookup> _mockOfficerAgentUserLookup = new(MockBehavior.Loose);
    private readonly Mock<IAppDataMiscMasterLookup> _mockAppDataMiscLookup = new(MockBehavior.Loose);
    private readonly Mock<ILogger<SubmitResolutionCommandHandler>> _mockLogger = new(MockBehavior.Loose);

    private SubmitResolutionCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object, _mockIpService.Object,
            _mockTzService.Object, _mockOutbox.Object, _mockMediator.Object, _mockMapper.Object,
            _mockComplaintQueryRepo.Object, _mockOfficerAgentUserLookup.Object,
            _mockAppDataMiscLookup.Object, _mockLogger.Object);

    private void SetupHappyPath(int newId = 1)
    {
        _mockMapper
            .Setup(m => m.Map<global::SalesManagement.Domain.Entities.ComplaintResolution>(It.IsAny<SubmitResolutionCommand>()))
            .Returns(new global::SalesManagement.Domain.Entities.ComplaintResolution());

        _mockIpService.Setup(s => s.GetUserId()).Returns(1);
        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
        _mockTzService.Setup(s => s.GetSystemTimeZone()).Returns("India Standard Time");
        _mockTzService.Setup(s => s.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new global::SalesManagement.Domain.Entities.MiscMaster { Id = 10 });

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<global::SalesManagement.Domain.Entities.ComplaintResolution>()))
            .ReturnsAsync(newId);

        _mockOutbox
            .Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // ----- Dynamic 'Resolution Submitted' InApp recipient defaults -----
        _mockIpService.Setup(s => s.GetUserName()).Returns("test-user");

        _mockAppDataMiscLookup
            .Setup(l => l.GetMiscMasterByNameAsync(
                MiscEnumEntity.NotifEventTypeMiscType, MiscEnumEntity.NotifEventTypeCreate))
            .ReturnsAsync(new Contracts.Dtos.Lookups.Inventory.MiscMasterLookupDto { Id = 1046, Code = "Create" });

        _mockComplaintQueryRepo
            .Setup(r => r.GetComplaintAgentIdsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<int>());

        _mockOfficerAgentUserLookup
            .Setup(l => l.GetMarketingOfficerChainByAgentIdsAsync(
                It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MoChainRow>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var command = new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace defective material"
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Resolution submitted successfully.");
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewId()
    {
        SetupHappyPath(newId: 42);
        var command = new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace"
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);
        result.Data.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateOnce()
    {
        SetupHappyPath();
        var command = new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace"
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<global::SalesManagement.Domain.Entities.ComplaintResolution>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath();
        var command = new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace"
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "COMPLAINT_RESOLUTION_SUBMIT"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------------
    // Auto-close: ClosureStatus → Closed when type-specific "done" signal is present.
    // ---------------------------------------------------------------------------

    private const int ClosedStatusId = 999;

    private global::SalesManagement.Domain.Entities.ComplaintResolution
        SetupAutoCloseScenario(string resolutionTypeCode)
    {
        var captured = new global::SalesManagement.Domain.Entities.ComplaintResolution();

        _mockMapper
            .Setup(m => m.Map<global::SalesManagement.Domain.Entities.ComplaintResolution>(It.IsAny<SubmitResolutionCommand>()))
            .Returns(captured);

        _mockIpService.Setup(s => s.GetUserId()).Returns(7);
        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
        _mockTzService.Setup(s => s.GetSystemTimeZone()).Returns("UTC");
        _mockTzService.Setup(s => s.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

        // Resolution type lookup
        _mockMiscRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new MiscMasterDto { Id = 1, Code = resolutionTypeCode });

        // Closed status lookup
        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.ClosureStatus, MiscEnumEntity.ClosureStatusClosed))
            .ReturnsAsync(new global::SalesManagement.Domain.Entities.MiscMaster
            {
                Id = ClosedStatusId, Code = MiscEnumEntity.ClosureStatusClosed
            });

        // Open status lookup (for the auto-default-Open path)
        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.ClosureStatus, MiscEnumEntity.ClosureStatusOpen))
            .ReturnsAsync(new global::SalesManagement.Domain.Entities.MiscMaster { Id = 100, Code = MiscEnumEntity.ClosureStatusOpen });

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<global::SalesManagement.Domain.Entities.ComplaintResolution>()))
            .ReturnsAsync(1);

        _mockOutbox
            .Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return captured;
    }

    [Fact]
    public async Task Handle_NoActionType_AutoClosesAlways()
    {
        var captured = SetupAutoCloseScenario(MiscEnumEntity.ResolutionNoAction);

        await CreateSut().Handle(new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1, ResolutionTypeId = 3, ResolutionSummary = "Nothing to do"
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().Be(ClosedStatusId);
        captured.ClosedBy.Should().Be(7);
        captured.ClosedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_CreditNoteWithFinanceReference_AutoCloses()
    {
        var captured = SetupAutoCloseScenario(MiscEnumEntity.ResolutionCreditNote);

        await CreateSut().Handle(new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1, ResolutionTypeId = 3, ResolutionSummary = "Credit issued",
            CreditAmount = 1000m, FinanceReference = "CN-2026-001"
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().Be(ClosedStatusId);
    }

    [Fact]
    public async Task Handle_CreditNoteWithoutFinanceReference_DoesNotAutoClose()
    {
        var captured = SetupAutoCloseScenario(MiscEnumEntity.ResolutionCreditNote);

        await CreateSut().Handle(new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1, ResolutionTypeId = 3, ResolutionSummary = "Credit pending",
            CreditAmount = 1000m, FinanceReference = null
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().NotBe(ClosedStatusId);
    }

    [Fact]
    public async Task Handle_ReplacementWithDispatchReference_AutoCloses()
    {
        var captured = SetupAutoCloseScenario(MiscEnumEntity.ResolutionReplacement);

        await CreateSut().Handle(new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1, ResolutionTypeId = 3, ResolutionSummary = "Replacement sent",
            ReplacementQuantity = 5m, DispatchReference = "DA-2026-001"
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().Be(ClosedStatusId);
    }

    [Fact]
    public async Task Handle_ReprocessWithActionDescription_AutoCloses()
    {
        var captured = SetupAutoCloseScenario(MiscEnumEntity.ResolutionReprocess);

        await CreateSut().Handle(new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1, ResolutionTypeId = 3, ResolutionSummary = "Reprocessed",
            ActionDescription = "Re-spun lot 7 through machine 12"
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().Be(ClosedStatusId);
    }

    [Fact]
    public async Task Handle_SalesReturnType_DoesNotAutoClose()
    {
        // Sales Return auto-close is the SalesReturn-receipt hook (separate workstream).
        var captured = SetupAutoCloseScenario(MiscEnumEntity.ResolutionSalesReturn);

        await CreateSut().Handle(new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1, ResolutionTypeId = 3, ResolutionSummary = "Awaiting goods receipt",
            ReturnQuantity = 5m, ReturnLocationId = 1
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().NotBe(ClosedStatusId);
    }

    // ---------------------------------------------------------------------------
    // Dynamic 'Resolution Submitted' InApp recipient (agent → MO chain).
    // Static WorkFlow_GetUserId can't resolve per-complaint, so the handler
    // resolves the agent-MO recipient in C# and passes it via
    // NotificationCreatedEvent.OverrideTargetUserIds.
    // ---------------------------------------------------------------------------

    private List<NotificationCreatedEvent> CaptureScheduledNotifications()
    {
        var captured = new List<NotificationCreatedEvent>();
        _mockOutbox
            .Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback<object, Guid, CancellationToken>((evt, _, _) =>
            {
                if (evt is NotificationCreatedEvent n)
                    captured.Add(n);
            })
            .Returns(Task.CompletedTask);
        return captured;
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesResolutionSubmittedNotification()
    {
        SetupHappyPath();
        var notifications = CaptureScheduledNotifications();

        await CreateSut().Handle(new SubmitResolutionCommand
        {
            ComplaintHeaderId = 7, ResolutionTypeId = 3, ResolutionSummary = "Replace"
        }, CancellationToken.None);

        notifications.Should().ContainSingle(n =>
            n.ModuleName == MiscEnumEntity.NotifModuleResolutionSubmitted &&
            n.ModuleTransactionId == 7 &&
            n.param1 == "7");
    }

    [Fact]
    public async Task Handle_AgentMoResolved_SetsOverrideTargetUserIds()
    {
        SetupHappyPath();
        _mockComplaintQueryRepo
            .Setup(r => r.GetComplaintAgentIdsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<int> { 34 });
        _mockOfficerAgentUserLookup
            .Setup(l => l.GetMarketingOfficerChainByAgentIdsAsync(
                It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MoChainRow> { new() { AgentId = 34, MoUserId = 37 } });
        var notifications = CaptureScheduledNotifications();

        await CreateSut().Handle(new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1, ResolutionTypeId = 3, ResolutionSummary = "Replace"
        }, CancellationToken.None);

        var resolutionNotif = notifications.Single(n =>
            n.ModuleName == MiscEnumEntity.NotifModuleResolutionSubmitted);
        resolutionNotif.OverrideTargetUserIds.Should().BeEquivalentTo(new[] { 37 });
    }

    [Fact]
    public async Task Handle_NoAgentMoResolved_OverrideTargetUserIdsIsNull()
    {
        SetupHappyPath(); // defaults: empty agent ids + empty MO chain
        var notifications = CaptureScheduledNotifications();

        await CreateSut().Handle(new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1, ResolutionTypeId = 3, ResolutionSummary = "Replace"
        }, CancellationToken.None);

        var resolutionNotif = notifications.Single(n =>
            n.ModuleName == MiscEnumEntity.NotifModuleResolutionSubmitted);
        resolutionNotif.OverrideTargetUserIds.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NotificationFailure_DoesNotFailResolution()
    {
        SetupHappyPath();
        _mockComplaintQueryRepo
            .Setup(r => r.GetComplaintAgentIdsAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("lookup down"));

        var result = await CreateSut().Handle(new SubmitResolutionCommand
        {
            ComplaintHeaderId = 1, ResolutionTypeId = 3, ResolutionSummary = "Replace"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
