using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.CreateSalesOrderTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrderTypeMaster.Commands;

public class CreateSalesOrderTypeMasterCommandHandlerTests
{
    private readonly Mock<ISalesOrderTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private CreateSalesOrderTypeMasterCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

    private static CreateSalesOrderTypeMasterCommand ValidCommand() => new()
    {
        SoTypeId = 1,
        TaxTypeId = 2,
        TypeName = "Normal Sales Order",
        Description = "Standard order type"
    };

    private void SetupMapper(CreateSalesOrderTypeMasterCommand cmd)
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOrderTypeMaster>(cmd))
            .Returns(new SalesManagement.Domain.Entities.SalesOrderTypeMaster
            {
                SoTypeId = cmd.SoTypeId,
                TaxTypeId = cmd.TaxTypeId,
                TypeName = cmd.TypeName,
                Description = cmd.Description
            });
    }

    private void SetupCreateAsync(int returnId = 1)
    {
        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesOrderTypeMaster>()))
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
            r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesOrderTypeMaster>()),
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
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALESORDERTYPEMASTER_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
