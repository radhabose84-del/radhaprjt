using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.UpdateSalesQuotation;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesQuotation.Commands;

public sealed class UpdateSalesQuotationCommandHandlerTests
{
    private readonly Mock<ISalesQuotationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesQuotationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private UpdateSalesQuotationCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsResult()
    {
        _mockMapper
            .Setup(m => m.Map<SalesQuotationHeader>(It.IsAny<object>()))
            .Returns(new SalesQuotationHeader { Id = 1 });
        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<SalesQuotationHeader>()))
            .ReturnsAsync(1);

        var command = new UpdateSalesQuotationCommand { Id = 1, CustomerId = 1, PaymentTermId = 1, DeliveryTermId = 1, IsActive = 1 };
        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        _mockMapper
            .Setup(m => m.Map<SalesQuotationHeader>(It.IsAny<object>()))
            .Returns(new SalesQuotationHeader { Id = 1 });
        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<SalesQuotationHeader>()))
            .ReturnsAsync(1);

        var command = new UpdateSalesQuotationCommand { Id = 1, CustomerId = 1, PaymentTermId = 1, DeliveryTermId = 1, IsActive = 1 };
        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALESQUOTATION_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
