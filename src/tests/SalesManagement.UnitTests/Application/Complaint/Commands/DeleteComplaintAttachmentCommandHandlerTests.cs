using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Commands.DeleteAttachment;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.Complaint.Commands;

public sealed class DeleteComplaintAttachmentCommandHandlerTests
{
    private readonly Mock<IComplaintCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private DeleteComplaintAttachmentCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_AttachmentNotFound_ThrowsExceptionRules()
    {
        _mockQueryRepo
            .Setup(r => r.GetAttachmentFilePathAsync(99))
            .ReturnsAsync((string?)null);

        Func<Task> act = async () => await CreateSut().Handle(
            new DeleteComplaintAttachmentCommand(99), CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*Attachment not found*");
    }

    [Fact]
    public async Task Handle_ExistingAttachment_ReturnsTrue()
    {
        _mockQueryRepo
            .Setup(r => r.GetAttachmentFilePathAsync(1))
            .ReturnsAsync("some/nonexistent/path.pdf");

        _mockCommandRepo
            .Setup(r => r.DeleteAttachmentAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(
            new DeleteComplaintAttachmentCommand(1), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExistingAttachment_PublishesAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetAttachmentFilePathAsync(1))
            .ReturnsAsync("some/nonexistent/path.pdf");

        _mockCommandRepo
            .Setup(r => r.DeleteAttachmentAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateSut().Handle(
            new DeleteComplaintAttachmentCommand(1), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "COMPLAINT_ATTACHMENT_DELETE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
