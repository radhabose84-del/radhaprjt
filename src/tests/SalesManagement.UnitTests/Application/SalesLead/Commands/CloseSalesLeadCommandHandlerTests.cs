using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Commands.CloseSalesLead;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesLead.Commands;

public class CloseSalesLeadCommandHandlerTests
{
    private readonly Mock<ISalesLeadCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesLeadQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
    private readonly Mock<ITimeZoneService> _mockTimeZone = new(MockBehavior.Loose);

    public CloseSalesLeadCommandHandlerTests()
    {
        _mockTimeZone.Setup(t => t.GetSystemTimeZone()).Returns("India Standard Time");
        _mockTimeZone.Setup(t => t.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);
    }

    private CloseSalesLeadCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object,
            _mockMapper.Object, _mockTimeZone.Object);

    private void SetupMapper()
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesLead>(It.IsAny<CloseSalesLeadCommand>()))
            .Returns((CloseSalesLeadCommand cmd) => new SalesManagement.Domain.Entities.SalesLead
            {
                Id = cmd.Id,
                ClosureTypeId = cmd.ClosureTypeId,
                ClosureReasonId = cmd.ClosureReasonId,
                ConvertWonLeadToId = cmd.ConvertWonLeadToId,
                ClosureRemarks = cmd.ClosureRemarks
            });
    }

    private void SetupHappyPath(bool isWon, int returnId = 1)
    {
        SetupMapper();
        _mockQueryRepo.Setup(r => r.IsWonClosureTypeAsync(It.IsAny<int>())).ReturnsAsync(isWon);
        _mockCommandRepo
            .Setup(r => r.CloseAsync(It.IsAny<SalesManagement.Domain.Entities.SalesLead>()))
            .ReturnsAsync(returnId);
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static CloseSalesLeadCommand LostCommand() => new()
    {
        Id = 1, ClosureTypeId = 2, ClosureReasonId = 5, ClosureRemarks = "Customer chose competitor"
    };

    private static CloseSalesLeadCommand WonCommand() => new()
    {
        Id = 1, ClosureTypeId = 1, ConvertWonLeadToId = 9, ClosureRemarks = "Customer confirmed order"
    };

    [Fact]
    public async Task Handle_NonWonClosure_ReturnsSuccess()
    {
        SetupHappyPath(isWon: false);

        var result = await CreateSut().Handle(LostCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NonWonClosure_ClearsConvertWonTarget()
    {
        SetupHappyPath(isWon: false);
        SalesManagement.Domain.Entities.SalesLead? captured = null;
        _mockCommandRepo
            .Setup(r => r.CloseAsync(It.IsAny<SalesManagement.Domain.Entities.SalesLead>()))
            .Callback<SalesManagement.Domain.Entities.SalesLead>(e => captured = e)
            .ReturnsAsync(1);

        var cmd = LostCommand();
        cmd.ConvertWonLeadToId = 9; // stray value should be cleared for non-Won
        await CreateSut().Handle(cmd, CancellationToken.None);

        captured!.ConvertWonLeadToId.Should().BeNull();
        captured.ClosureReasonId.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WonClosure_ClearsClosureReason()
    {
        SetupHappyPath(isWon: true);
        SalesManagement.Domain.Entities.SalesLead? captured = null;
        _mockCommandRepo
            .Setup(r => r.CloseAsync(It.IsAny<SalesManagement.Domain.Entities.SalesLead>()))
            .Callback<SalesManagement.Domain.Entities.SalesLead>(e => captured = e)
            .ReturnsAsync(1);

        var cmd = WonCommand();
        cmd.ClosureReasonId = 5; // stray value should be cleared for Won
        await CreateSut().Handle(cmd, CancellationToken.None);

        captured!.ClosureReasonId.Should().BeNull();
        captured.ConvertWonLeadToId.Should().Be(9);
    }

    [Fact]
    public async Task Handle_SetsClosureDate()
    {
        SetupHappyPath(isWon: false);
        SalesManagement.Domain.Entities.SalesLead? captured = null;
        _mockCommandRepo
            .Setup(r => r.CloseAsync(It.IsAny<SalesManagement.Domain.Entities.SalesLead>()))
            .Callback<SalesManagement.Domain.Entities.SalesLead>(e => captured = e)
            .ReturnsAsync(1);

        await CreateSut().Handle(LostCommand(), CancellationToken.None);

        captured!.ClosureDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_CallsCloseAsync_Once()
    {
        SetupHappyPath(isWon: false);

        await CreateSut().Handle(LostCommand(), CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.CloseAsync(It.IsAny<SalesManagement.Domain.Entities.SalesLead>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PublishesAuditEvent_Once()
    {
        SetupHappyPath(isWon: false);

        await CreateSut().Handle(LostCommand(), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALES_LEAD_CLOSE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
