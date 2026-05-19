using SalesManagement.Domain.Entities;
using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Events.Notifications;
using Contracts.Interfaces;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Complaint.Commands.CreateComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.Complaint.Commands;

public sealed class CreateComplaintCommandHandlerTests
{
    private readonly Mock<IComplaintCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<ILogger<CreateComplaintCommandHandler>> _mockLogger = new(MockBehavior.Loose);
    private readonly Mock<IAppDataMiscMasterLookup> _mockAppDataMiscLookup = new(MockBehavior.Loose);
    private readonly Mock<IOfficerAgentUserLookup> _mockOfficerAgentUserLookup = new(MockBehavior.Loose);

    private CreateComplaintCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object, _mockDocSeqLookup.Object,
            _mockIpService.Object, _mockOutbox.Object, _mockMediator.Object, _mockMapper.Object,
            _mockLogger.Object, _mockAppDataMiscLookup.Object, _mockOfficerAgentUserLookup.Object);

    private void SetupHappyPath(int newId = 1)
    {
        _mockMapper
            .Setup(m => m.Map<ComplaintHeader>(It.IsAny<CreateComplaintCommand>()))
            .Returns(new ComplaintHeader());

        _mockMapper
            .Setup(m => m.Map<ComplaintDetail>(It.IsAny<CreateComplaintDetailDto>()))
            .Returns(new ComplaintDetail());

        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new global::SalesManagement.Domain.Entities.MiscMaster { Id = 10 });

        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

        _mockDocSeqLookup
            .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(5);

        _mockDocSeqLookup
            .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
            .ReturnsAsync(new List<string> { "CMP-0001" });

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<ComplaintHeader>(), It.IsAny<int>()))
            .ReturnsAsync(newId);

        // Default: no agent ids resolved → OverrideTargetUserIds stays null (fallback).
        // Individual tests override these to assert the dynamic-recipient path.
        _mockQueryRepo
            .Setup(r => r.GetComplaintAgentIdsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<int>());

        _mockOfficerAgentUserLookup
            .Setup(l => l.GetMarketingOfficerChainByAgentIdsAsync(
                It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MoChainRow>());

        _mockOutbox
            .Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // EventType MiscMaster lookup — returns a stub MiscMasterLookupDto with a fake Id
        _mockAppDataMiscLookup
            .Setup(l => l.GetMiscMasterByNameAsync(
                MiscEnumEntity.NotifEventTypeMiscType, MiscEnumEntity.NotifEventTypeCreate))
            .ReturnsAsync(new MiscMasterLookupDto { Id = 1046, Code = "Create" });
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var command = new CreateComplaintCommand
        {
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            Details = new List<CreateComplaintDetailDto>()
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Complaint created successfully.");
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewId()
    {
        SetupHappyPath(newId: 42);
        var command = new CreateComplaintCommand
        {
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            Details = new List<CreateComplaintDetailDto>()
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Data.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateOnce()
    {
        SetupHappyPath();
        var command = new CreateComplaintCommand
        {
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            Details = new List<CreateComplaintDetailDto>()
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<ComplaintHeader>(), It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath();
        var command = new CreateComplaintCommand
        {
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            Details = new List<CreateComplaintDetailDto>()
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "Create" &&
                    e.ActionCode == "COMPLAINT_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoTransactionType_ThrowsExceptionRules()
    {
        SetupHappyPath();
        _mockDocSeqLookup
            .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((int?)null);

        var command = new CreateComplaintCommand
        {
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            Details = new List<CreateComplaintDetailDto>()
        };

        Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*Transaction Type*");
    }

    // ============================================================
    // Event 1 — InApp notification publish tests (Phase 1)
    // ============================================================

    [Fact]
    public async Task Handle_ValidCommand_PublishesNewComplaintInAppEvent()
    {
        SetupHappyPath();
        var command = new CreateComplaintCommand
        {
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            Details = new List<CreateComplaintDetailDto>()
        };

        await CreateSut().Handle(command, CancellationToken.None);

        // Verify a NotificationCreatedEvent was scheduled with ModuleName="New Complaint"
        // EventTypeId comes from the mocked IAppDataMiscMasterLookup stub (Id=1046).
        _mockOutbox.Verify(
            o => o.ScheduleAsync(
                It.Is<NotificationCreatedEvent>(e =>
                    e.ModuleName == MiscEnumEntity.NotifModuleNewComplaint &&
                    e.EventTypeId == 1046),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_AgentMoResolved_SetsOverrideTargetUserIds()
    {
        SetupHappyPath();
        _mockQueryRepo
            .Setup(r => r.GetComplaintAgentIdsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<int> { 34 });
        _mockOfficerAgentUserLookup
            .Setup(l => l.GetMarketingOfficerChainByAgentIdsAsync(
                It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MoChainRow>
            {
                new() { AgentId = 34, MoUserId = 37, ReportToId = null }
            });

        var command = new CreateComplaintCommand
        {
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            Details = new List<CreateComplaintDetailDto>()
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockOutbox.Verify(
            o => o.ScheduleAsync(
                It.Is<NotificationCreatedEvent>(e =>
                    e.ModuleName == MiscEnumEntity.NotifModuleNewComplaint &&
                    e.OverrideTargetUserIds != null &&
                    e.OverrideTargetUserIds.Count == 1 &&
                    e.OverrideTargetUserIds[0] == 37),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoAgentMoResolved_LeavesOverrideTargetUserIdsNull()
    {
        SetupHappyPath();
        // Default SetupHappyPath returns empty list → fallback path.

        var command = new CreateComplaintCommand
        {
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            Details = new List<CreateComplaintDetailDto>()
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockOutbox.Verify(
            o => o.ScheduleAsync(
                It.Is<NotificationCreatedEvent>(e =>
                    e.ModuleName == MiscEnumEntity.NotifModuleNewComplaint &&
                    e.OverrideTargetUserIds == null),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NotificationPublishFails_DoesNotBreakComplaintCreation()
    {
        SetupHappyPath();

        // Make the InApp publish throw, but the workflow publish succeed.
        // Since both call ScheduleAsync, we use a sequence: first call (InApp) fails, rest succeed.
        var callCount = 0;
        _mockOutbox
            .Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns<object, Guid, CancellationToken>((_, _, _) =>
            {
                callCount++;
                if (callCount == 1) throw new InvalidOperationException("Outbox down");
                return Task.CompletedTask;
            });

        var command = new CreateComplaintCommand
        {
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            Details = new List<CreateComplaintDetailDto>()
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        // Complaint creation must still succeed
        result.IsSuccess.Should().BeTrue();
    }
}
