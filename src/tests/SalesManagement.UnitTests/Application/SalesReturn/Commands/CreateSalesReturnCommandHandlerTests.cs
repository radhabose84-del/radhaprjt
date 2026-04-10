using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Commands.CreateSalesReturn;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesReturn.Commands;

public sealed class CreateSalesReturnCommandHandlerTests
{
    private readonly Mock<ISalesReturnCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

    private CreateSalesReturnCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object,
            _mockDocSeqLookup.Object, _mockIpService.Object, _mockMediator.Object, _mockMapper.Object);

    private void SetupHappyPath(int newId = 1)
    {
        _mockMapper
            .Setup(m => m.Map<SalesReturnHeader>(It.IsAny<CreateSalesReturnCommand>()))
            .Returns(new SalesReturnHeader());

        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 10 });

        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

        _mockDocSeqLookup
            .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(5);

        _mockDocSeqLookup
            .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
            .ReturnsAsync(new List<string> { "SR-0001" });

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesReturnHeader>(), It.IsAny<int>()))
            .ReturnsAsync(newId);

        _mockQueryRepo
            .Setup(r => r.GetReturnProgressAsync(It.IsAny<int>()))
            .ReturnsAsync((10, 10));

        _mockCommandRepo
            .Setup(r => r.UpdateComplaintResolutionReturnStatusAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()))
            .Returns(Task.CompletedTask);

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static CreateSalesReturnCommand ValidCommand() => new()
    {
        ReturnDate = new DateOnly(2026, 1, 1),
        ComplaintHeaderId = 1,
        CustomerId = 1,
        WarehouseId = 1,
        BinId = 1
    };

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Sales Return created successfully.");
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewId()
    {
        SetupHappyPath(newId: 42);
        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        result.Data.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateOnce()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<SalesReturnHeader>(), It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "Create" &&
                    e.ActionCode == "SALES_RETURN_CREATE"),
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

        Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*Transaction Type*");
    }
}
