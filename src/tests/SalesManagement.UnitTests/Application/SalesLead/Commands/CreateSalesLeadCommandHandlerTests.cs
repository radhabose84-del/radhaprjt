using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Commands.CreateSalesLead;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesLead.Commands;

public class CreateSalesLeadCommandHandlerTests
{
    private readonly Mock<ISalesLeadCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesLeadQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
    private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

    private CreateSalesLeadCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object,
            _mockMediator.Object, _mockMapper.Object, _mockDocSeqLookup.Object,
            _mockIpService.Object, _mockAccessFilter.Object);

    private CreateSalesLeadCommand ValidCommand() => new()
    {
        ContactName = "Jane Doe",
        MobileNumber = "9876543210",
        MarketingOfficerId = 1,
        ContactId = 5,
        InteractionDate = DateTimeOffset.UtcNow
    };

    private void SetupMapper(CreateSalesLeadCommand cmd)
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesLead>(cmd))
            .Returns(new SalesManagement.Domain.Entities.SalesLead
            {
                ContactName = cmd.ContactName,
                MobileNumber = cmd.MobileNumber,
                MarketingOfficerId = cmd.MarketingOfficerId,
                ContactId = cmd.ContactId
            });
    }

    private void SetupHappyPath(int returnId = 1)
    {
        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

        _mockDocSeqLookup
            .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(5);

        _mockDocSeqLookup
            .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
            .ReturnsAsync(new List<string> { "SL-0001" });

        _mockCommandRepo
            .Setup(r => r.CreateAsync(
                It.IsAny<SalesManagement.Domain.Entities.SalesLead>(),
                It.IsAny<int>(),
                It.IsAny<SalesManagement.Domain.Entities.SalesContact?>()))
            .ReturnsAsync(returnId);

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_WithExistingContact_ReturnsSuccess()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupHappyPath(1);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewEntityId()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupHappyPath(42);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsLeadNoInResponse()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupHappyPath(1);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.LeadNo.Should().Be("SL-0001");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateAsync_Once()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupHappyPath(1);

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.CreateAsync(
                It.IsAny<SalesManagement.Domain.Entities.SalesLead>(),
                It.IsAny<int>(),
                It.IsAny<SalesManagement.Domain.Entities.SalesContact?>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupHappyPath(1);

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALES_LEAD_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoContactId_WithContactName_PassesNewContactToRepository()
    {
        var command = new CreateSalesLeadCommand
        {
            ContactName = "New Contact",
            MobileNumber = "1234567890",
            MarketingOfficerId = 1,
            ContactId = null,
            InteractionDate = DateTimeOffset.UtcNow
        };

        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesLead>(command))
            .Returns(new SalesManagement.Domain.Entities.SalesLead
            {
                ContactName = command.ContactName,
                MobileNumber = command.MobileNumber,
                MarketingOfficerId = command.MarketingOfficerId
            });

        _mockQueryRepo
            .Setup(r => r.GetPrimaryContactTypeIdAsync())
            .ReturnsAsync(1);

        SetupHappyPath(1);

        await CreateSut().Handle(command, CancellationToken.None);

        // Auto-create flow: handler builds a SalesContact and hands it to the repository
        // (not persisted via a separate ISalesContactCommandRepository call).
        _mockCommandRepo.Verify(
            r => r.CreateAsync(
                It.IsAny<SalesManagement.Domain.Entities.SalesLead>(),
                It.IsAny<int>(),
                It.Is<SalesManagement.Domain.Entities.SalesContact?>(c => c != null && c.ContactName == "New Contact")),
            Times.Once);
    }
}
