using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Commands.CreateStoTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.StoTypeMaster.Commands;

public class CreateStoTypeMasterCommandHandlerTests
{
    private readonly Mock<IStoTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private CreateStoTypeMasterCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

    private CreateStoTypeMasterCommand ValidCommand() => new()
    {
        StoTypeCode = "STO001",
        StoTypeName = "Standard STO",
        Description = "Standard Stock Transfer Order",
        PgiMovementTypeId = 1,
        GrMovementTypeId = 2
    };

    private void SetupMapper(CreateStoTypeMasterCommand cmd)
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.StoTypeMaster>(cmd))
            .Returns(new SalesManagement.Domain.Entities.StoTypeMaster
            {
                StoTypeCode = cmd.StoTypeCode,
                StoTypeName = cmd.StoTypeName,
                Description = cmd.Description,
                PgiMovementTypeId = cmd.PgiMovementTypeId,
                GrMovementTypeId = cmd.GrMovementTypeId
            });
    }

    private void SetupCreateAsync(int returnId = 1)
    {
        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.StoTypeMaster>()))
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
            r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.StoTypeMaster>()),
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
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "STO_TYPE_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
