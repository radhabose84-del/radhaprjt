using MediatR;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Commands.DeleteTripSheet;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.TripSheet.Commands;

public class DeleteTripSheetCommandHandlerTests
{
    private readonly Mock<ITripSheetCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private DeleteTripSheetCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMediator.Object);

    private void SetupHappyPath(int id = 1, bool result = true)
    {
        _mockCommandRepo
            .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsTrue()
    {
        SetupHappyPath(1);
        var result = await CreateSut().Handle(new DeleteTripSheetCommand(1), CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidId_CallsSoftDeleteAsync_Once()
    {
        SetupHappyPath(1);
        await CreateSut().Handle(new DeleteTripSheetCommand(1), CancellationToken.None);
        _mockCommandRepo.Verify(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidId_PublishesAuditLogEvent_Once()
    {
        SetupHappyPath(1);
        await CreateSut().Handle(new DeleteTripSheetCommand(1), CancellationToken.None);
        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "TRIPSHEET_DELETE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
