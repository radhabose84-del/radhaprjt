using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.RequestRework;

using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintDepartmentFeedback.Commands;

public sealed class RequestReworkCommandHandlerTests
{
    private readonly Mock<IComplaintDepartmentFeedbackCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private RequestReworkCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object, _mockMediator.Object);

    private void SetupHappyPath(int result = 1)
    {
        _mockQueryRepo
            .Setup(r => r.GetReworkInfoAsync(It.IsAny<int>()))
            .ReturnsAsync((1, 10));

        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new global::SalesManagement.Domain.Entities.MiscMaster { Id = 20 });

        _mockCommandRepo
            .Setup(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        _mockQueryRepo
            .Setup(r => r.GetAssignmentIdByFeedbackIdAsync(It.IsAny<int>()))
            .ReturnsAsync(5);

        _mockCommandRepo
            .Setup(r => r.UpdateAssignmentStatusAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var command = new RequestReworkCommand
        {
            FeedbackId = 1,
            ReworkReason = "Need more detail on root cause"
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Rework requested successfully.");
    }

    [Fact]
    public async Task Handle_ValidCommand_IncrementsReworkCount()
    {
        _mockQueryRepo
            .Setup(r => r.GetReworkInfoAsync(1))
            .ReturnsAsync((2, 10));

        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new global::SalesManagement.Domain.Entities.MiscMaster { Id = 20 });

        _mockCommandRepo
            .Setup(r => r.UpdateStatusAsync(1, 20, "Need rework", 3))
            .ReturnsAsync(1);

        _mockQueryRepo
            .Setup(r => r.GetAssignmentIdByFeedbackIdAsync(1))
            .ReturnsAsync(5);

        _mockCommandRepo
            .Setup(r => r.UpdateAssignmentStatusAsync(5, It.IsAny<int>()))
            .ReturnsAsync(1);

        var command = new RequestReworkCommand { FeedbackId = 1, ReworkReason = "Need rework" };
        var result = await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.UpdateStatusAsync(1, 20, "Need rework", 3),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath();
        var command = new RequestReworkCommand { FeedbackId = 1, ReworkReason = "Fix it" };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "COMPLAINT_FEEDBACK_REWORK"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
