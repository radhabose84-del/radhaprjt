using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.DeleteAttachment;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintDepartmentFeedback.Commands;

public sealed class DeleteFeedbackAttachmentCommandHandlerTests
{
    private readonly Mock<IComplaintDepartmentFeedbackCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private DeleteFeedbackAttachmentCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_AttachmentNotFound_ThrowsExceptionRules()
    {
        _mockQueryRepo
            .Setup(r => r.GetAttachmentFilePathAsync(99))
            .ReturnsAsync((string?)null);

        Func<Task> act = async () => await CreateSut().Handle(
            new DeleteFeedbackAttachmentCommand(99), CancellationToken.None);

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

        var result = await CreateSut().Handle(
            new DeleteFeedbackAttachmentCommand(1), CancellationToken.None);

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

        await CreateSut().Handle(
            new DeleteFeedbackAttachmentCommand(1), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "FEEDBACK_ATTACHMENT_DELETE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
