using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Commands.CreateTripSheet;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.TripSheet.Commands;

public class CreateTripSheetCommandHandlerTests
{
    private readonly Mock<ITripSheetCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ITripSheetQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private CreateTripSheetCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockDocSeq.Object,
            _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

    private static CreateTripSheetCommand ValidCommand() => new()
    {
        TripDate = new DateOnly(2026, 1, 15),
        VehicleNo = "KA01AB1234",
        Remarks = "Trip remarks"
    };

    private void SetupHappyPath(int newId = 1)
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.TripSheetHeader>(It.IsAny<CreateTripSheetCommand>()))
            .Returns((CreateTripSheetCommand c) => new SalesManagement.Domain.Entities.TripSheetHeader
            {
                TripDate = c.TripDate,
                VehicleNo = c.VehicleNo,
                Remarks = c.Remarks
            });
        _mockIp.Setup(s => s.GetUnitId()).Returns(1);
        _mockDocSeq
            .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(5);
        _mockDocSeq
            .Setup(d => d.GenerateDocumentNumber(5))
            .ReturnsAsync(new List<string> { "TS001" });
        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.TripSheetHeader>(), 5))
            .ReturnsAsync(newId);
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath(1);
        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewId()
    {
        SetupHappyPath(42);
        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        result.Data.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateAsync_Once()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.TripSheetHeader>(), 5),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "TRIPSHEET_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
