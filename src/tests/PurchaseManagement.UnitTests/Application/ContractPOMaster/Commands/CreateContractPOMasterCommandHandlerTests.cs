using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Application.ContractPOMaster.Commands.Create;
using PurchaseManagement.Application.ContractPOMaster.Dto;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.ContractPOMaster.Commands;

public class CreateContractPOMasterCommandHandlerTests
{
    private readonly Mock<IContractPOMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IContractPOMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
    private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
    private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);

    private CreateContractPOMasterCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
            _mockDocSeq.Object, _mockIp.Object, _mockOutbox.Object, _mockMisc.Object);

    private static CreateContractPOMasterCommand ValidCommand()
    {
        var now = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        return new CreateContractPOMasterCommand
        {
            ContractDate = now,
            VendorId = 1,
            CurrencyId = 1,
            ValidityFrom = now,
            ValidityTo = now.AddMonths(6),
            Remarks = "contract",
            Details = new List<CreateContractPODetailItem>
            {
                new() { ItemSno = 1, ItemId = 10, UOMId = 1, ContractQuantity = 100m, ContractRate = 10m }
            }
        };
    }

    private void SetupHappyPath(int newId = 1)
    {
        _mockMapper.Setup(m => m.Map<ContractPOHeader>(It.IsAny<CreateContractPOMasterCommand>()))
            .Returns(new ContractPOHeader());
        _mockMapper.Setup(m => m.Map<ContractPODetail>(It.IsAny<CreateContractPODetailItem>()))
            .Returns(new ContractPODetail());
        _mockMisc.Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 10 });
        _mockIp.Setup(s => s.GetUnitId()).Returns(1);
        _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(5);
        _mockDocSeq.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
            .ReturnsAsync(new List<string> { "CPO001" });
        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<ContractPOHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractPOHeader { Id = newId, ContractPONumber = "CPO001" });
        _mockCommandRepo
            .Setup(r => r.GetByIdContractPOWorkFlowAsync(newId))
            .ReturnsAsync((ContractPOMasterWorkFlowDto)null!);
        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(newId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractPOHeaderDto { Id = newId, ContractPONumber = "CPO001", VendorId = 1 });
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
        result.ContractPONumber.Should().Be("CPO001");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateAsync_Once()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<ContractPOHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
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
