using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesQuotationAmendment.Commands;

public class CreateSalesQuotationAmendmentCommandHandlerTests
{
    private readonly Mock<ISalesQuotationAmendmentCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<ILogger<CreateSalesQuotationAmendmentCommandHandler>> _mockLogger = new(MockBehavior.Loose);
    private readonly Mock<IAppDataMiscMasterLookup> _mockAppMisc = new(MockBehavior.Loose);
    private readonly Mock<IOfficerAgentUserLookup> _mockOfficer = new(MockBehavior.Loose);

    private CreateSalesQuotationAmendmentCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockOutbox.Object, _mockIp.Object, _mockMediator.Object,
            _mockLogger.Object, _mockAppMisc.Object, _mockOfficer.Object);

    private static CreateSalesQuotationAmendmentCommand ValidCommand() => new()
    {
        SalesQuotationHeaderId = 1,
        Reason = "Price revision",
        AmendmentDetails = new List<CreateSalesQuotationAmendmentDetailDto>
        {
            new() { SalesQuotationDetailId = 10, NewQuantity = 5m, NetRate = 90m, TotalAmount = 450m, TaxAmount = 22.5m }
        }
    };

    private void SetupHappyPath(int newId = 5)
    {
        var sqHeader = new SalesManagement.Domain.Entities.SalesQuotationHeader
        {
            Id = 1,
            QuotationNo = "Q1",
            RevisionNumber = 0,
            SalesQuotationDetails = new List<SalesManagement.Domain.Entities.SalesQuotationDetail>
            {
                new() { Id = 10, ItemId = 5, Quantity = 10m, ExMillRate = 100m, Discount = 0m, HSNId = 1, TaxPercentage = 5m }
            }
        };

        _mockCommandRepo.Setup(r => r.GetSalesQuotationEntityAsync(1)).ReturnsAsync(sqHeader);
        _mockCommandRepo
            .Setup(r => r.CreateAsync(
                It.IsAny<SalesManagement.Domain.Entities.SalesQuotationAmendmentHeader>(),
                It.IsAny<List<SalesManagement.Domain.Entities.SalesQuotationAmendmentDetail>>()))
            .ReturnsAsync(newId);
        _mockCommandRepo.Setup(r => r.GetByIdAmendmentWorkFlowAsync(newId)).ReturnsAsync(new AmendmentWorkFlowDto());
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath(5);
        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent_Once()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALESQUOTATIONAMENDMENT_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuotationNotFound_ThrowsException()
    {
        _mockCommandRepo
            .Setup(r => r.GetSalesQuotationEntityAsync(99))
            .ReturnsAsync((SalesManagement.Domain.Entities.SalesQuotationHeader?)null);

        var cmd = ValidCommand();
        cmd.SalesQuotationHeaderId = 99;

        var act = async () => await CreateSut().Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*quotation not found*");
    }
}
