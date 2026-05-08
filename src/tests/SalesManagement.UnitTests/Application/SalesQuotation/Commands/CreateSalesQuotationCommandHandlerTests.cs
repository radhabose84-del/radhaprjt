using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesQuotation.Commands;

public sealed class CreateSalesQuotationCommandHandlerTests
{
    private readonly Mock<ISalesQuotationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesQuotationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<IOutboxEventPublisher> _mockOutboxPublisher = new(MockBehavior.Loose);
    private readonly Mock<IPartyDetailLookup> _mockPartyDetailLookup = new(MockBehavior.Loose);
    private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
    private readonly Mock<IHSNLookup> _mockHsnLookup = new(MockBehavior.Loose);
    private readonly Mock<ILogger<CreateSalesQuotationCommandHandler>> _mockLogger = new(MockBehavior.Loose);
    private readonly Mock<IAppDataMiscMasterLookup> _mockAppDataMiscLookup = new(MockBehavior.Loose);
    private readonly Mock<IConfiguration> _mockConfiguration = new(MockBehavior.Loose);
    private readonly Mock<IOfficerAgentUserLookup> _mockOfficerAgentUserLookup = new(MockBehavior.Loose);

    private CreateSalesQuotationCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object,
            _mockMapper.Object, _mockMediator.Object, _mockDocSeqLookup.Object, _mockIpService.Object,
            _mockOutboxPublisher.Object, _mockPartyDetailLookup.Object, _mockItemLookup.Object,
            _mockHsnLookup.Object, _mockLogger.Object, _mockAppDataMiscLookup.Object,
            _mockConfiguration.Object, _mockOfficerAgentUserLookup.Object);

    private void SetupHappyPath(int newId = 1)
    {
        _mockMapper
            .Setup(m => m.Map<SalesQuotationHeader>(It.IsAny<object>()))
            .Returns(new SalesQuotationHeader { CustomerId = 1 });

        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 10, Description = "Pending" });

        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

        _mockDocSeqLookup
            .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(5);

        _mockDocSeqLookup
            .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
            .ReturnsAsync(new List<string> { "SQ-0001" });

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesQuotationHeader>(), It.IsAny<int>()))
            .ReturnsAsync(newId);

        _mockCommandRepo
            .Setup(r => r.GetByIdSalesQuotationWorkFlowAsync(It.IsAny<int>()))
            .ReturnsAsync(new SalesQuotationWorkFlowDto { Id = newId, QuotationNo = "SQ-0001" });
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewId()
    {
        SetupHappyPath(42);
        var command = new CreateSalesQuotationCommand { CustomerId = 1, PaymentTermId = 1, DeliveryTermId = 1 };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath(1);
        var command = new CreateSalesQuotationCommand { CustomerId = 1, PaymentTermId = 1, DeliveryTermId = 1 };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "Create" &&
                    e.ActionCode == "SALESQUOTATION_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CreateReturnsZero_ThrowsExceptionRules()
    {
        SetupHappyPath(0);
        var command = new CreateSalesQuotationCommand { CustomerId = 1, PaymentTermId = 1, DeliveryTermId = 1 };

        Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>();
    }
}
