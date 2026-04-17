using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrderAmendment;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrderAmendment.Commands;

public sealed class CreateSalesOrderAmendmentCommandHandlerTests
{
    private readonly Mock<ISalesOrderAmendmentCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private CreateSalesOrderAmendmentCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockOutbox.Object, _mockIpService.Object, _mockMediator.Object);

    private void SetupHappyPath(int newId = 1)
    {
        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

        var soHeader = new SalesOrderHeader
        {
            Id = 10,
            SalesOrderNo = "SO001",
            RevisionNumber = 0,
            SalesOrderDetails = new List<SalesOrderDetail>
            {
                new() { Id = 100, QtyInBags = 50, ExMillRate = 100m }
            }
        };
        _mockCommandRepo
            .Setup(r => r.GetSalesOrderEntityAsync(10))
            .ReturnsAsync(soHeader);

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesOrderAmendmentHeader>(), It.IsAny<List<SalesOrderAmendmentDetail>>(), It.IsAny<List<SalesOrderAmendmentDiscount>>()))
            .ReturnsAsync(newId);

        _mockCommandRepo
            .Setup(r => r.GetByIdAmendmentWorkFlowAsync(newId))
            .ReturnsAsync(new AmendmentWorkFlowDto());
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath(42);
        var command = new CreateSalesOrderAmendmentCommand
        {
            SalesOrderHeaderId = 10,
            Reason = "Test amendment",
            AmendmentDetails = new List<SalesManagement.Application.SalesOrder.Dto.CreateSalesOrderAmendmentDetailDto>
            {
                new() { SalesOrderDetailId = 100, NewQtyInBags = 60 }
            }
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(42);
    }

    [Fact]
    public async Task Handle_SalesOrderNotFound_ThrowsExceptionRules()
    {
        _mockCommandRepo
            .Setup(r => r.GetSalesOrderEntityAsync(999))
            .ReturnsAsync((SalesOrderHeader?)null);

        var command = new CreateSalesOrderAmendmentCommand { SalesOrderHeaderId = 999 };

        Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath(1);
        var command = new CreateSalesOrderAmendmentCommand
        {
            SalesOrderHeaderId = 10,
            Reason = "Test",
            AmendmentDetails = new List<SalesManagement.Application.SalesOrder.Dto.CreateSalesOrderAmendmentDetailDto>
            {
                new() { SalesOrderDetailId = 100, NewQtyInBags = 60 }
            }
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALESORDERAMENDMENT_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidDetailId_ThrowsExceptionRules()
    {
        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
        _mockCommandRepo
            .Setup(r => r.GetSalesOrderEntityAsync(10))
            .ReturnsAsync(new SalesOrderHeader
            {
                Id = 10,
                SalesOrderNo = "SO001",
                RevisionNumber = 0,
                SalesOrderDetails = new List<SalesOrderDetail>()
            });

        var command = new CreateSalesOrderAmendmentCommand
        {
            SalesOrderHeaderId = 10,
            AmendmentDetails = new List<SalesManagement.Application.SalesOrder.Dto.CreateSalesOrderAmendmentDetailDto>
            {
                new() { SalesOrderDetailId = 999 }
            }
        };

        Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*detail Id 999 not found*");
    }
}
