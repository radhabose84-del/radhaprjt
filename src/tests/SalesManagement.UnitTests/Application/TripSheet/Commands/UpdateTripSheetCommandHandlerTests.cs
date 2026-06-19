using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Commands.UpdateTripSheet;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.TripSheet.Commands;

public class UpdateTripSheetCommandHandlerTests
{
    private readonly Mock<ITripSheetCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ITripSheetQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private UpdateTripSheetCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object,
            _mockMediator.Object, _mockMapper.Object);

    private static UpdateTripSheetCommand ValidCommand() => new()
    {
        Id = 1,
        TripDate = new DateOnly(2026, 1, 15),
        VehicleNo = "KA01AB1234",
        Remarks = "Updated",
        IsActive = 1
    };

    private void SetupHappyPath(int returnId = 1)
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.TripSheetHeader>(It.IsAny<UpdateTripSheetCommand>()))
            .Returns((UpdateTripSheetCommand c) => new SalesManagement.Domain.Entities.TripSheetHeader
            {
                Id = c.Id,
                TripDate = c.TripDate,
                VehicleNo = c.VehicleNo,
                Remarks = c.Remarks
            });
        _mockIp.Setup(s => s.GetUnitId()).Returns(1);
        _mockCommandRepo
            .Setup(r => r.UpdateAsync(
                It.IsAny<SalesManagement.Domain.Entities.TripSheetHeader>(),
                It.IsAny<List<SalesManagement.Domain.Entities.TripSheetDetail>>()))
            .ReturnsAsync(returnId);
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_EntityExists_ReturnsSuccess()
    {
        SetupHappyPath();
        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EntityExists_CallsUpdateAsync_Once()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        _mockCommandRepo.Verify(
            r => r.UpdateAsync(
                It.IsAny<SalesManagement.Domain.Entities.TripSheetHeader>(),
                It.IsAny<List<SalesManagement.Domain.Entities.TripSheetDetail>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "TRIPSHEET_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
