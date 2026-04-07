using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Commands.CreateSalesLead;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesLead.Commands;

public class CreateSalesLeadCommandHandlerTests
{
    private readonly Mock<ISalesLeadCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesLeadQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesContactCommandRepository> _mockContactCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private CreateSalesLeadCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockContactCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

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

    private void SetupCreateAsync(int returnId = 1)
    {
        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesLead>()))
            .ReturnsAsync(returnId);
    }

    private void SetupPublishAudit()
    {
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_WithExistingContact_ReturnsSuccess()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupCreateAsync(1);
        SetupPublishAudit();

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewEntityId()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupCreateAsync(42);
        SetupPublishAudit();

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Data.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateAsync_Once()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupCreateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesLead>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupCreateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALES_LEAD_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoContactId_WithContactName_AutoCreatesSalesContact()
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

        _mockContactCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesContact>()))
            .ReturnsAsync(99);

        SetupCreateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockContactCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesContact>()),
            Times.Once);
    }
}
