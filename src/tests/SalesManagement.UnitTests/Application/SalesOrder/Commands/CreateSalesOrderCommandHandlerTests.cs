using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrder.Commands;

public sealed class CreateSalesOrderCommandHandlerTests
{
    private readonly Mock<ISalesOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
    private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
    private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
    private readonly Mock<ILogger<CreateSalesOrderCommandHandler>> _mockLogger = new(MockBehavior.Loose);
    private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);

    private CreateSalesOrderCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object,
            _mockIpService.Object, _mockCompanyLookup.Object, _mockUnitLookup.Object,
            _mockDocSeqLookup.Object, _mockLogger.Object, _mockOutbox.Object);

    private void SetupHappyPath(int newId = 1)
    {
        _mockMapper
            .Setup(m => m.Map<SalesOrderHeader>(It.IsAny<CreateSalesOrderDto>()))
            .Returns(new SalesOrderHeader());

        _mockDocSeqLookup
            .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
            .ReturnsAsync(new List<string> { "SO-0001" });

        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesOrderHeader>(), It.IsAny<int>()))
            .ReturnsAsync(newId);

        _mockCommandRepo
            .Setup(r => r.GetByIdSalesOrderWorkFlowAsync(It.IsAny<int>()))
            .ReturnsAsync(new SalesOrderWorkFlowDto());

        _mockOutbox
            .Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static CreateSalesOrderCommand ValidCommand() => new()
    {
        SalesOrderDetails = new CreateSalesOrderDto
        {
            SalesOrderTypeId = 1,
            SalesGroupId = 1,
            PartyId = 1,
            UnitId = 1,
            FreightTypeId = 1,
            EnquiryType = 1
        }
    };

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Sales Order created successfully.");
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
            r => r.CreateAsync(It.IsAny<SalesOrderHeader>(), It.IsAny<int>()),
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
                    e.ActionCode == "SALESORDER_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NullSalesOrderTypeId_ThrowsExceptionRules()
    {
        SetupHappyPath();
        var command = ValidCommand();
        command.SalesOrderDetails!.SalesOrderTypeId = null;

        Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*SalesOrderTypeId*");
    }
}
