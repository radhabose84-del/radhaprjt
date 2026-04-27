using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.ComplaintResolution.Commands.UpdateResolution;
using SalesManagement.Application.ComplaintResolution.Dto;
using SalesManagement.Application.MiscMaster.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintResolution.Commands;

public sealed class UpdateResolutionCommandHandlerTests
{
    private readonly Mock<IComplaintResolutionCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IComplaintResolutionQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ITimeZoneService> _mockTzService = new(MockBehavior.Loose);
    private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

    private UpdateResolutionCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object,
            _mockIpService.Object, _mockTzService.Object, _mockOutbox.Object,
            _mockMediator.Object, _mockMapper.Object);

    private void SetupHappyPath(int result = 1)
    {
        _mockMapper
            .Setup(m => m.Map<global::SalesManagement.Domain.Entities.ComplaintResolution>(It.IsAny<UpdateResolutionCommand>()))
            .Returns(new global::SalesManagement.Domain.Entities.ComplaintResolution { Id = 1 });

        _mockIpService.Setup(s => s.GetUserId()).Returns(1);
        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
        _mockTzService.Setup(s => s.GetSystemTimeZone()).Returns("India Standard Time");
        _mockTzService.Setup(s => s.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new global::SalesManagement.Domain.Entities.MiscMaster { Id = 10 });

        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<global::SalesManagement.Domain.Entities.ComplaintResolution>()))
            .ReturnsAsync(result);

        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new ComplaintResolutionDto { Id = 1, ComplaintHeaderId = 10 });

        _mockOutbox
            .Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var command = new UpdateResolutionCommand
        {
            Id = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Updated resolution",
            IsActive = 1
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Resolution updated successfully.");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUpdateOnce()
    {
        SetupHappyPath();
        var command = new UpdateResolutionCommand
        {
            Id = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Updated",
            IsActive = 1
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.UpdateAsync(It.IsAny<global::SalesManagement.Domain.Entities.ComplaintResolution>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath();
        var command = new UpdateResolutionCommand
        {
            Id = 1,
            ResolutionTypeId = 3,
            ResolutionSummary = "Updated",
            IsActive = 1
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "COMPLAINT_RESOLUTION_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------------
    // Auto-close: ClosureStatus → Closed when type-specific "done" signal is present.
    // ---------------------------------------------------------------------------

    private const int ClosedStatusId = 999;

    private (Mock<IComplaintResolutionCommandRepository> repo,
             global::SalesManagement.Domain.Entities.ComplaintResolution captured)
        SetupAutoCloseScenario(string resolutionTypeCode)
    {
        var captured = new global::SalesManagement.Domain.Entities.ComplaintResolution { Id = 1 };

        _mockMapper
            .Setup(m => m.Map<global::SalesManagement.Domain.Entities.ComplaintResolution>(It.IsAny<UpdateResolutionCommand>()))
            .Returns(captured);

        _mockIpService.Setup(s => s.GetUserId()).Returns(7);
        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
        _mockTzService.Setup(s => s.GetSystemTimeZone()).Returns("UTC");
        _mockTzService.Setup(s => s.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

        // Mock the resolution type lookup
        _mockMiscRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new MiscMasterDto { Id = 1, Code = resolutionTypeCode });

        // Mock the Closed status lookup
        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.ClosureStatus, MiscEnumEntity.ClosureStatusClosed))
            .ReturnsAsync(new global::SalesManagement.Domain.Entities.MiscMaster
            {
                Id = ClosedStatusId, Code = MiscEnumEntity.ClosureStatusClosed
            });

        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<global::SalesManagement.Domain.Entities.ComplaintResolution>()))
            .ReturnsAsync(1);

        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new ComplaintResolutionDto { Id = 1, ComplaintHeaderId = 5 });

        return (_mockCommandRepo, captured);
    }

    [Fact]
    public async Task Handle_NoActionType_AutoClosesAlways()
    {
        var (_, captured) = SetupAutoCloseScenario(MiscEnumEntity.ResolutionNoAction);

        await CreateSut().Handle(new UpdateResolutionCommand
        {
            Id = 1, ResolutionTypeId = 3, ResolutionSummary = "Nothing to do", IsActive = 1
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().Be(ClosedStatusId);
        captured.ClosedBy.Should().Be(7);
        captured.ClosedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_CreditNoteWithFinanceReference_AutoCloses()
    {
        var (_, captured) = SetupAutoCloseScenario(MiscEnumEntity.ResolutionCreditNote);

        await CreateSut().Handle(new UpdateResolutionCommand
        {
            Id = 1, ResolutionTypeId = 3, ResolutionSummary = "Credit issued",
            CreditAmount = 1000m, FinanceReference = "CN-2026-001", IsActive = 1
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().Be(ClosedStatusId);
        captured.ClosedBy.Should().Be(7);
    }

    [Fact]
    public async Task Handle_CreditNoteWithoutFinanceReference_DoesNotAutoClose()
    {
        var (_, captured) = SetupAutoCloseScenario(MiscEnumEntity.ResolutionCreditNote);

        await CreateSut().Handle(new UpdateResolutionCommand
        {
            Id = 1, ResolutionTypeId = 3, ResolutionSummary = "Credit pending",
            CreditAmount = 1000m, FinanceReference = null, IsActive = 1
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().NotBe(ClosedStatusId);
        captured.ClosedBy.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReplacementWithDispatchReference_AutoCloses()
    {
        var (_, captured) = SetupAutoCloseScenario(MiscEnumEntity.ResolutionReplacement);

        await CreateSut().Handle(new UpdateResolutionCommand
        {
            Id = 1, ResolutionTypeId = 3, ResolutionSummary = "Replacement sent",
            ReplacementQuantity = 5m, DispatchReference = "DA-2026-001", IsActive = 1
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().Be(ClosedStatusId);
    }

    [Fact]
    public async Task Handle_ReprocessWithActionDescription_AutoCloses()
    {
        var (_, captured) = SetupAutoCloseScenario(MiscEnumEntity.ResolutionReprocess);

        await CreateSut().Handle(new UpdateResolutionCommand
        {
            Id = 1, ResolutionTypeId = 3, ResolutionSummary = "Reprocessed",
            ActionDescription = "Re-spun the lot through machine 12", IsActive = 1
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().Be(ClosedStatusId);
    }

    [Fact]
    public async Task Handle_SalesReturnType_DoesNotAutoClose()
    {
        // Sales Return auto-close is the SalesReturn-receipt hook (separate workstream).
        var (_, captured) = SetupAutoCloseScenario(MiscEnumEntity.ResolutionSalesReturn);

        await CreateSut().Handle(new UpdateResolutionCommand
        {
            Id = 1, ResolutionTypeId = 3, ResolutionSummary = "Awaiting goods receipt",
            ReturnQuantity = 5m, ReturnLocationId = 1, IsActive = 1
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().NotBe(ClosedStatusId);
    }

    [Fact]
    public async Task Handle_UnknownTypeCode_DoesNotAutoClose()
    {
        var (_, captured) = SetupAutoCloseScenario("SomeFutureType");

        await CreateSut().Handle(new UpdateResolutionCommand
        {
            Id = 1, ResolutionTypeId = 3, ResolutionSummary = "Unknown", IsActive = 1
        }, CancellationToken.None);

        captured.ClosureStatusId.Should().NotBe(ClosedStatusId);
    }
}
