using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Commands.DeleteStoTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.StoTypeMaster.Commands;

public class DeleteStoTypeMasterCommandHandlerTests
{
    private readonly Mock<IStoTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private DeleteStoTypeMasterCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMediator.Object);

    private void SetupSoftDelete(int id = 1)
    {
        _mockCommandRepo
            .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SetupPublishAudit()
    {
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsTrue()
    {
        SetupSoftDelete(1);
        SetupPublishAudit();

        var result = await CreateSut().Handle(new DeleteStoTypeMasterCommand(1), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidId_CallsSoftDeleteAsync_Once()
    {
        SetupSoftDelete(1);
        SetupPublishAudit();

        await CreateSut().Handle(new DeleteStoTypeMasterCommand(1), CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidId_PublishesAuditLogEvent_Once()
    {
        SetupSoftDelete(1);
        SetupPublishAudit();

        await CreateSut().Handle(new DeleteStoTypeMasterCommand(1), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "STO_TYPE_DELETE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
