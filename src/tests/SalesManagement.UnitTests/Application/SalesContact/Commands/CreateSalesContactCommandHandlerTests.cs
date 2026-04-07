using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.SalesContact.Commands.CreateSalesContact;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesContact.Commands;

public class CreateSalesContactCommandHandlerTests
{
    private readonly Mock<ISalesContactCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesContactQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private CreateSalesContactCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

    private CreateSalesContactCommand ValidCommand() => new()
    {
        ContactName = "John Doe",
        MobileNumber = "9876543210",
        ContactTypeId = 1,
        PartyId = 10,
        Email = "john@test.com",
        Remarks = "Test"
    };

    private void SetupMapper(CreateSalesContactCommand cmd)
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesContact>(cmd))
            .Returns(new SalesManagement.Domain.Entities.SalesContact
            {
                ContactName = cmd.ContactName,
                MobileNumber = cmd.MobileNumber,
                ContactTypeId = cmd.ContactTypeId
            });
    }

    private void SetupCreateAsync(int returnId = 1)
    {
        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesContact>()))
            .ReturnsAsync(returnId);
    }

    private void SetupPublishAudit()
    {
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
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
            r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesContact>()),
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
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALES_CONTACT_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
