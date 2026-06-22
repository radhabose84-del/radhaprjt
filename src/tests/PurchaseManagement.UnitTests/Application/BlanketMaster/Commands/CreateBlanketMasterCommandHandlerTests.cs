using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.BlanketMaster.Commands.Create;
using PurchaseManagement.Application.BlanketMaster.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Domain.Entities.BlanketMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.BlanketMaster.Commands;

public class CreateBlanketMasterCommandHandlerTests
{
    private readonly Mock<IBlanketMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IBlanketMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
    private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
    private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
    private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
    private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);

    private CreateBlanketMasterCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object,
            _mockIp.Object, _mockDocSeq.Object, _mockOutbox.Object, _mockMisc.Object);

    private static CreateBlanketMasterCommand ValidCommand()
    {
        var now = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        return new CreateBlanketMasterCommand
        {
            BlanketDate = now,
            VendorId = 1,
            CurrencyId = 1,
            ProcurementTypeId = 1,
            ValidityFrom = now,
            ValidityTo = now.AddMonths(6),
            Remarks = "blanket",
            Details = new List<CreateBlanketMasterDetailItem>
            {
                new()
                {
                    ItemSno = 1, ItemId = 10, UOMId = 1, EstimatedQuantity = 100m, Rate = 10m,
                    Schedules = new List<CreateBlanketMasterScheduleItem>
                    {
                        new() { ScheduleNo = 1, ScheduleDate = now.AddDays(30), ScheduleQuantity = 50m }
                    }
                }
            }
        };
    }

    private void SetupHappyPath(int newId = 1)
    {
        _mockIp.Setup(s => s.GetUnitId()).Returns(1);
        _mockMisc.Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 10 });
        _mockMapper.Setup(m => m.Map<BlanketHeader>(It.IsAny<CreateBlanketMasterCommand>()))
            .Returns(new BlanketHeader());
        _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(5);
        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<BlanketHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlanketHeader { Id = newId, BlanketNumber = "BLK001" });
        _mockCommandRepo
            .Setup(r => r.GetByIdBlanketWorkFlowAsync(newId))
            .ReturnsAsync((BlanketMasterWorkFlowDto)null!);
        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(newId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlanketHeaderDto { Id = newId, BlanketNumber = "BLK001" });
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsDto()
    {
        SetupHappyPath(1);
        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.BlanketNumber.Should().Be("BLK001");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateAsync_Once()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<BlanketHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent_Once()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "BLANKET_MASTER_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
