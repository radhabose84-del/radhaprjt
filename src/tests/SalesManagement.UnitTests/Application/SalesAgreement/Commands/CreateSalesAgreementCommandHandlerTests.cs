using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Commands.CreateSalesAgreement;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesAgreement.Commands;

public class CreateSalesAgreementCommandHandlerTests
{
    private readonly Mock<ISalesAgreementCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
    private readonly Mock<ICompanyLookup> _mockCompany = new(MockBehavior.Loose);
    private readonly Mock<IUnitLookup> _mockUnit = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<ILogger<CreateSalesAgreementCommandHandler>> _mockLogger = new(MockBehavior.Loose);

    private CreateSalesAgreementCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMiscRepo.Object, _mockDocSeq.Object, _mockIp.Object,
            _mockCompany.Object, _mockUnit.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

    // AgentPOAttachment is null → handler skips all file/company/unit logic.
    private static CreateSalesAgreementCommand ValidCommand() => new()
    {
        ValidFrom = new DateOnly(2026, 1, 1),
        ValidTo = new DateOnly(2026, 12, 31),
        CustomerId = 1,
        SalesGroupId = 1,
        PaymentTermsId = 1,
        SalesAgreementDetails = new()
        {
            new() { ItemId = 1, AgreedRate = 10m, TotalQty = 100m }
        }
    };

    private void SetupHappyPath(int newId = 1)
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesAgreementHeader>(It.IsAny<CreateSalesAgreementCommand>()))
            .Returns(new SalesManagement.Domain.Entities.SalesAgreementHeader());
        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 10 });
        _mockIp.Setup(s => s.GetUnitId()).Returns(1);
        _mockDocSeq
            .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(5);
        _mockDocSeq.Setup(d => d.GenerateDocumentNumber(5)).ReturnsAsync(new List<string> { "SA001" });
        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesAgreementHeader>(), 5))
            .ReturnsAsync(newId);
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath(1);
        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewId()
    {
        SetupHappyPath(42);
        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        result.Data.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
    {
        SetupHappyPath();
        await CreateSut().Handle(ValidCommand(), CancellationToken.None);
        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALESAGREEMENT_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
