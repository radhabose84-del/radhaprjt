using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Commands.DeleteSalesAgreement;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesAgreement.Commands;

public class DeleteSalesAgreementCommandHandlerTests
{
    private readonly Mock<ISalesAgreementCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private DeleteSalesAgreementCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMiscRepo.Object, _mockMediator.Object);

    private void SetupCancelled()
    {
        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 20 });
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsTrue()
    {
        SetupCancelled();
        _mockCommandRepo.Setup(r => r.CancelAsync(1, 20, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(new DeleteSalesAgreementCommand(1), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidId_PublishesCancelAuditEvent_Once()
    {
        SetupCancelled();
        _mockCommandRepo.Setup(r => r.CancelAsync(1, 20, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateSut().Handle(new DeleteSalesAgreementCommand(1), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALESAGREEMENT_CANCEL"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsException()
    {
        SetupCancelled();
        _mockCommandRepo.Setup(r => r.CancelAsync(99, 20, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = async () => await CreateSut().Handle(new DeleteSalesAgreementCommand(99), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*not found*");
    }
}
