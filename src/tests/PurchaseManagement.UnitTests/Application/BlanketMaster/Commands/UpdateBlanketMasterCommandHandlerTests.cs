using AutoMapper;
using MediatR;
using PurchaseManagement.Application.BlanketMaster.Commands.Update;
using PurchaseManagement.Application.BlanketMaster.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Domain.Entities.BlanketMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.BlanketMaster.Commands;

public class UpdateBlanketMasterCommandHandlerTests
{
    private readonly Mock<IBlanketMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IBlanketMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private UpdateBlanketMasterCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

    private static UpdateBlanketMasterCommand ValidCommand()
    {
        var now = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        return new UpdateBlanketMasterCommand
        {
            Id = 1,
            VendorId = 1,
            CurrencyId = 1,
            ProcurementTypeId = 1,
            ValidityFrom = now,
            ValidityTo = now.AddMonths(6),
            StatusId = 10,
            Remarks = "updated",
            IsActive = 1,
            Details = new List<UpdateBlanketMasterDetailItem>
            {
                new()
                {
                    Id = 0, ItemSno = 1, ItemId = 10, UOMId = 1, EstimatedQuantity = 100m, Rate = 10m,
                    Schedules = new List<UpdateBlanketMasterScheduleItem>
                    {
                        new() { Id = 0, ScheduleNo = 1, ScheduleDate = now.AddDays(30), ScheduleQuantity = 50m }
                    }
                }
            }
        };
    }

    private void SetupHappyPath(int id = 1)
    {
        _mockMapper.Setup(m => m.Map<BlanketHeader>(It.IsAny<UpdateBlanketMasterCommand>()))
            .Returns(new BlanketHeader { Id = id });
        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<BlanketHeader>(), It.IsAny<List<BlanketDetail>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlanketHeader { Id = id, BlanketNumber = "BLK001" });
        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlanketHeaderDto { Id = id, BlanketNumber = "BLK001" });
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsDto()
    {
        SetupHappyPath();
        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUpdateAsync_Once()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        _mockCommandRepo.Verify(
            r => r.UpdateAsync(It.IsAny<BlanketHeader>(), It.IsAny<List<BlanketDetail>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent_Once()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "BLANKET_MASTER_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
