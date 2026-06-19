using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Application.ContractPOMaster.Commands.Update;
using PurchaseManagement.Application.ContractPOMaster.Dto;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.ContractPOMaster.Commands;

public class UpdateContractPOMasterCommandHandlerTests
{
    private readonly Mock<IContractPOMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IContractPOMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private UpdateContractPOMasterCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    private static UpdateContractPOMasterCommand ValidCommand()
    {
        var now = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        return new UpdateContractPOMasterCommand
        {
            Id = 1,
            VendorId = 1,
            CurrencyId = 1,
            ValidityFrom = now,
            ValidityTo = now.AddMonths(6),
            StatusId = 10,
            Remarks = "updated",
            IsActive = 1,
            Details = new List<UpdateContractPODetailItem>
            {
                new() { Id = 0, ItemSno = 1, ItemId = 10, UOMId = 1, ContractQuantity = 100m, ContractRate = 10m }
            }
        };
    }

    private void SetupHappyPath(int id = 1)
    {
        _mockMapper.Setup(m => m.Map<ContractPOHeader>(It.IsAny<UpdateContractPOMasterCommand>()))
            .Returns(new ContractPOHeader { Id = id });
        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<ContractPOHeader>(), It.IsAny<List<ContractPODetail>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractPOHeader { Id = id, ContractPONumber = "CPO001" });
        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractPOHeaderDto { Id = id, ContractPONumber = "CPO001" });
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
            r => r.UpdateAsync(It.IsAny<ContractPOHeader>(), It.IsAny<List<ContractPODetail>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent_Once()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        _mockMediator.Verify(
            m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
