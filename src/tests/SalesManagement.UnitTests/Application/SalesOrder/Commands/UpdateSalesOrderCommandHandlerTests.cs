using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Commands.UpdateSalesOrder;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrder.Commands;

public sealed class UpdateSalesOrderCommandHandlerTests
{
    private readonly Mock<ISalesOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
    private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
    private readonly Mock<ILogger<UpdateSalesOrderCommandHandler>> _mockLogger = new(MockBehavior.Loose);

    private UpdateSalesOrderCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object,
            _mockMediator.Object, _mockIpService.Object, _mockCompanyLookup.Object,
            _mockUnitLookup.Object, _mockLogger.Object);

    private void SetupHappyPath(int result = 1)
    {
        _mockMapper
            .Setup(m => m.Map<SalesOrderHeader>(It.IsAny<UpdateSalesOrderCommand>()))
            .Returns(new SalesOrderHeader());

        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<SalesOrderHeader>()))
            .ReturnsAsync(result);

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static UpdateSalesOrderCommand ValidCommand() => new()
    {
        Id = 1,
        SalesGroupId = 1,
        UnitId = 1,
        PartyId = 1,
        PaymentTermsId = 1,
        FreightTypeId = 1,
        EnquiryType = 1,
        IsActive = 1
    };

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Sales Order updated successfully.");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUpdateOnce()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.UpdateAsync(It.IsAny<SalesOrderHeader>()),
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
                    e.ActionDetail == "Update" &&
                    e.ActionCode == "SALESORDER_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
