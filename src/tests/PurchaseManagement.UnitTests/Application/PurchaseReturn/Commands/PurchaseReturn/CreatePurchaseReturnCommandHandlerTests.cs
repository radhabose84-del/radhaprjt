using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CreatePurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.SubmitPurchaseReturn;
using PurchaseManagement.Domain.Entities.PurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Commands.PurchaseReturn;

public sealed class CreatePurchaseReturnCommandHandlerTests
{
    private readonly Mock<IPurchaseReturnCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
    private readonly Mock<IPurchaseReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private CreatePurchaseReturnCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockDocSeqLookup.Object,
            _mockIp.Object, _mockMapper.Object, _mockMediator.Object);

    private void SetupHappyPath(int newId = 1)
    {
        var entity = PurchaseReturnBuilders.ValidHeaderEntity(newId);
        _mockMapper.Setup(m => m.Map<PurchaseReturnHeader>(It.IsAny<CreatePurchaseReturnCommand>())).Returns(entity);
        _mockQueryRepo.Setup(r => r.GetStatusIdByCodeAsync(It.IsAny<string>())).ReturnsAsync(1);
        _mockIp.Setup(i => i.GetUnitId()).Returns(37);
        _mockDocSeqLookup
            .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(10);
        _mockDocSeqLookup
            .Setup(d => d.GenerateDocumentNumber(10))
            .ReturnsAsync(new List<string> { "RTV/2026/0001" });
        _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<PurchaseReturnHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _mockQueryRepo.Setup(r => r.GetByIdAsync(newId, It.IsAny<CancellationToken>())).ReturnsAsync(PurchaseReturnBuilders.ValidHeaderDto(newId));
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsDto()
    {
        SetupHappyPath();
        var result = await CreateSut().Handle(PurchaseReturnBuilders.ValidCreateCommand(), CancellationToken.None);
        result.Should().NotBeNull();
        result.RtvNumber.Should().Be("RTV/2026/0001");
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesResolvedTransactionTypeIdToCreate()
    {
        // Guards against the regression where the document sequence was never advanced:
        // the handler must forward the resolved transaction type id so the repository can
        // increment DocNo in the same transaction as the insert.
        SetupHappyPath();
        await CreateSut().Handle(PurchaseReturnBuilders.ValidCreateCommand(), CancellationToken.None);
        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<PurchaseReturnHeader>(), 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PostsStraightToApproval()
    {
        // Purchase Returns have no resting Draft state: create must immediately submit
        // the new RTV so it lands on the approval screen.
        SetupHappyPath(newId: 7);
        await CreateSut().Handle(PurchaseReturnBuilders.ValidCreateCommand(), CancellationToken.None);
        _mockMediator.Verify(
            m => m.Send(It.Is<SubmitPurchaseReturnCommand>(c => c.Id == 7), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PendingStatusMissing_ThrowsExceptionRules()
    {
        _mockMapper.Setup(m => m.Map<PurchaseReturnHeader>(It.IsAny<CreatePurchaseReturnCommand>()))
                   .Returns(PurchaseReturnBuilders.ValidHeaderEntity());
        _mockQueryRepo.Setup(r => r.GetStatusIdByCodeAsync(It.IsAny<string>())).ReturnsAsync((int?)null);

        Func<Task> act = async () => await CreateSut().Handle(PurchaseReturnBuilders.ValidCreateCommand(), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*Pending*");
    }

    [Fact]
    public async Task Handle_NoTransactionType_ThrowsExceptionRules()
    {
        _mockMapper.Setup(m => m.Map<PurchaseReturnHeader>(It.IsAny<CreatePurchaseReturnCommand>()))
                   .Returns(PurchaseReturnBuilders.ValidHeaderEntity());
        _mockQueryRepo.Setup(r => r.GetStatusIdByCodeAsync(It.IsAny<string>())).ReturnsAsync(1);
        _mockIp.Setup(i => i.GetUnitId()).Returns(37);
        _mockDocSeqLookup
            .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((int?)null);

        Func<Task> act = async () => await CreateSut().Handle(PurchaseReturnBuilders.ValidCreateCommand(), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*Transaction Type*");
    }
}
