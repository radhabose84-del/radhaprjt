using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.DeleteSalesOrderTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrderTypeMaster.Commands;

public class DeleteSalesOrderTypeMasterCommandHandlerTests
{
    private readonly Mock<ISalesOrderTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private DeleteSalesOrderTypeMasterCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMediator.Object);

    private void SetupSoftDelete(int id = 1, bool result = true)
    {
        _mockCommandRepo
            .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
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

        var result = await CreateSut().Handle(new DeleteSalesOrderTypeMasterCommand(1), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidId_CallsSoftDeleteAsync_Once()
    {
        SetupSoftDelete(1);
        SetupPublishAudit();

        await CreateSut().Handle(new DeleteSalesOrderTypeMasterCommand(1), CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidId_PublishesAuditLogEvent_Once()
    {
        SetupSoftDelete(1);
        SetupPublishAudit();

        await CreateSut().Handle(new DeleteSalesOrderTypeMasterCommand(1), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALESORDERTYPEMASTER_DELETE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
