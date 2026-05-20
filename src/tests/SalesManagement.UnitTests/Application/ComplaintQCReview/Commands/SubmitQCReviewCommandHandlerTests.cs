using SalesManagement.Domain.Entities;
using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Events.Notifications;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.ComplaintQCReview.Commands.SubmitQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintQCReview.Commands;

public sealed class SubmitQCReviewCommandHandlerTests
{
    private readonly Mock<IComplaintQCReviewCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IComplaintQCReviewQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ITimeZoneService> _mockTzService = new(MockBehavior.Loose);
    private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<ILogger<SubmitQCReviewCommandHandler>> _mockLogger = new(MockBehavior.Loose);
    private readonly Mock<IAppDataMiscMasterLookup> _mockAppDataMiscLookup = new(MockBehavior.Loose);
    private readonly Mock<IDepartmentUserLookup> _mockDepartmentUserLookup = new(MockBehavior.Loose);

    private SubmitQCReviewCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object,
            _mockIpService.Object, _mockTzService.Object, _mockOutbox.Object,
            _mockMediator.Object, _mockMapper.Object, _mockLogger.Object,
            _mockAppDataMiscLookup.Object, _mockDepartmentUserLookup.Object);

    private void SetupHappyPath(int newId = 1)
    {
        _mockMapper
            .Setup(m => m.Map<global::SalesManagement.Domain.Entities.ComplaintQCReview>(It.IsAny<SubmitQCReviewCommand>()))
            .Returns(new global::SalesManagement.Domain.Entities.ComplaintQCReview());

        _mockIpService.Setup(s => s.GetUserId()).Returns(1);
        _mockIpService.Setup(s => s.GetUserName()).Returns("test-user");
        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
        _mockTzService.Setup(s => s.GetSystemTimeZone()).Returns("India Standard Time");
        _mockTzService.Setup(s => s.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new global::SalesManagement.Domain.Entities.MiscMaster { Id = 10 });

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<global::SalesManagement.Domain.Entities.ComplaintQCReview>()))
            .ReturnsAsync(newId);

        _mockOutbox
            .Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // EventType MiscMaster lookup — returns a stub MiscMasterLookupDto with a fake Id
        _mockAppDataMiscLookup
            .Setup(l => l.GetMiscMasterByNameAsync(
                MiscEnumEntity.NotifEventTypeMiscType, MiscEnumEntity.NotifEventTypeCreate))
            .ReturnsAsync(new MiscMasterLookupDto { Id = 1046, Code = "Create" });

        // Default: no QC-reviewer resolves → OverrideTargetUserIds stays null (dispatcher fallback)
        _mockDepartmentUserLookup
            .Setup(l => l.GetActiveUserIdsByApprovalStepTargetTypeAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review complete",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("QC Review submitted successfully.");
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewId()
    {
        SetupHappyPath(newId: 42);
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);
        result.Data.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateOnce()
    {
        SetupHappyPath();
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<global::SalesManagement.Domain.Entities.ComplaintQCReview>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath();
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "COMPLAINT_QC_REVIEW_SUBMIT"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ============================================================
    // Event 3 — InApp notification publish tests (Phase 1)
    // ============================================================

    [Fact]
    public async Task Handle_ValidCommand_PublishesQcReviewSubmittedInAppEvent()
    {
        SetupHappyPath();
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        await CreateSut().Handle(command, CancellationToken.None);

        // EventTypeId comes from the mocked IAppDataMiscMasterLookup stub (Id=1046).
        _mockOutbox.Verify(
            o => o.ScheduleAsync(
                It.Is<NotificationCreatedEvent>(e =>
                    e.ModuleName == MiscEnumEntity.NotifModuleQcReviewSubmitted &&
                    e.EventTypeId == 1046),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NotificationPublishFails_DoesNotBreakSubmission()
    {
        SetupHappyPath();

        var callCount = 0;
        _mockOutbox
            .Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns<object, Guid, CancellationToken>((_, _, _) =>
            {
                callCount++;
                if (callCount == 1) throw new InvalidOperationException("Outbox down");
                return Task.CompletedTask;
            });

        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // ============================================================
    // Dynamic QC-reviewer recipient (AppData.ApprovalStepDepartmentMapping → users).
    // Static WorkFlow_GetUserId can't resolve the QC department team, so the handler
    // resolves it in C# and passes it via NotificationCreatedEvent.OverrideTargetUserIds.
    // ============================================================

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
    public async Task Handle_QcReviewersResolved_SetsOverrideTargetUserIds()
    {
        SetupHappyPath();
        _mockDepartmentUserLookup
            .Setup(l => l.GetActiveUserIdsByApprovalStepTargetTypeAsync(
                MiscEnumEntity.ComplaintQcReviewerTargetType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 379, 2443 });
        var notifications = CaptureScheduledNotifications();

        await CreateSut().Handle(new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        }, CancellationToken.None);

        var qcNotif = notifications.Single(n =>
            n.ModuleName == MiscEnumEntity.NotifModuleQcReviewSubmitted);
        qcNotif.OverrideTargetUserIds.Should().BeEquivalentTo(new[] { 379, 2443 });
    }

    [Fact]
    public async Task Handle_NoQcReviewersResolved_OverrideTargetUserIdsIsNull()
    {
        SetupHappyPath(); // default: empty QC user list
        var notifications = CaptureScheduledNotifications();

        await CreateSut().Handle(new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        }, CancellationToken.None);

        var qcNotif = notifications.Single(n =>
            n.ModuleName == MiscEnumEntity.NotifModuleQcReviewSubmitted);
        qcNotif.OverrideTargetUserIds.Should().BeNull();
    }

    // ============================================================
    // Phase 3 — 'Feedback Requested' fires one InApp per assignment
    // ============================================================

    [Fact]
    public async Task Handle_WithAssignments_PublishesOneFeedbackRequestedPerAssignee()
    {
        SetupHappyPath();
        var notifications = CaptureScheduledNotifications();

        await CreateSut().Handle(new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 7,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1),
            Assignments = new List<SubmitAssignmentDto>
            {
                new() { RoleId = 1, ResponsiblePersonId = 101, IsMandatory = true },
                new() { RoleId = 2, ResponsiblePersonId = 202, IsMandatory = false }
            }
        }, CancellationToken.None);

        var feedbackEvents = notifications
            .Where(n => n.ModuleName == MiscEnumEntity.NotifModuleFeedbackRequested)
            .ToList();
        feedbackEvents.Should().HaveCount(2);
        feedbackEvents.SelectMany(e => e.OverrideTargetUserIds!)
            .Should().BeEquivalentTo(new[] { 101, 202 });
    }

    [Fact]
    public async Task Handle_WithoutAssignments_DoesNotPublishFeedbackRequested()
    {
        SetupHappyPath();
        var notifications = CaptureScheduledNotifications();

        await CreateSut().Handle(new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        }, CancellationToken.None);

        notifications.Should().NotContain(n =>
            n.ModuleName == MiscEnumEntity.NotifModuleFeedbackRequested);
    }
}
