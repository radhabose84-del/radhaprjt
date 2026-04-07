using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.DeleteDispatchAddressMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.DispatchAddressMapping.Commands;

public class DeleteDispatchAddressMappingCommandHandlerTests
{
    private readonly Mock<IDispatchAddressMappingCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private DeleteDispatchAddressMappingCommandHandler CreateSut() =>
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

        var result = await CreateSut().Handle(new DeleteDispatchAddressMappingCommand(1), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidId_CallsSoftDeleteAsync_Once()
    {
        SetupSoftDelete(1);
        SetupPublishAudit();

        await CreateSut().Handle(new DeleteDispatchAddressMappingCommand(1), CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidId_PublishesAuditLogEvent_Once()
    {
        SetupSoftDelete(1);
        SetupPublishAudit();

        await CreateSut().Handle(new DeleteDispatchAddressMappingCommand(1), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "DISPATCH_ADDRESS_MAPPING_DELETE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentId_ThrowsExceptionRules()
    {
        _mockCommandRepo
            .Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = CreateSut();
        Func<Task> act = async () => await sut.Handle(
            new DeleteDispatchAddressMappingCommand(99), CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*not found*");
    }
}
