using SalesManagement.Domain.Entities;
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UpdateFeedback;

using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintDepartmentFeedback.Commands;

public sealed class UpdateComplaintDepartmentFeedbackCommandHandlerTests
{
    private readonly Mock<IComplaintDepartmentFeedbackCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IComplaintDepartmentFeedbackQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ITimeZoneService> _mockTzService = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

    private UpdateComplaintDepartmentFeedbackCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object,
            _mockIpService.Object, _mockTzService.Object, _mockMediator.Object, _mockMapper.Object);

    private void SetupHappyPath(int result = 1)
    {
        _mockMapper
            .Setup(m => m.Map<global::SalesManagement.Domain.Entities.ComplaintDepartmentFeedback>(
                It.IsAny<UpdateComplaintDepartmentFeedbackCommand>()))
            .Returns(new global::SalesManagement.Domain.Entities.ComplaintDepartmentFeedback { Id = 1 });

        _mockQueryRepo
            .Setup(r => r.GetReworkInfoAsync(It.IsAny<int>()))
            .ReturnsAsync((0, 10));

        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new global::SalesManagement.Domain.Entities.MiscMaster { Id = 10 });

        _mockIpService.Setup(s => s.GetUserId()).Returns(1);
        _mockTzService.Setup(s => s.GetSystemTimeZone()).Returns("India Standard Time");
        _mockTzService
            .Setup(s => s.GetCurrentTime(It.IsAny<string>()))
            .Returns(DateTimeOffset.UtcNow);

        _mockQueryRepo
            .Setup(r => r.GetAssignmentIdByFeedbackIdAsync(It.IsAny<int>()))
            .ReturnsAsync(5);

        _mockCommandRepo
            .Setup(r => r.UpdateAsync(
                It.IsAny<global::SalesManagement.Domain.Entities.ComplaintDepartmentFeedback>(),
                It.IsAny<ICollection<ComplaintFeedbackAttachment>?>()))
            .ReturnsAsync(result);

        _mockCommandRepo
            .Setup(r => r.UpdateAssignmentStatusAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var command = new UpdateComplaintDepartmentFeedbackCommand
        {
            Id = 1,
            CorrectiveAction = "Updated fix",
            IsActive = 1
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Feedback updated successfully.");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUpdateOnce()
    {
        SetupHappyPath();
        var command = new UpdateComplaintDepartmentFeedbackCommand
        {
            Id = 1,
            CorrectiveAction = "Updated fix",
            IsActive = 1
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.UpdateAsync(
                It.IsAny<global::SalesManagement.Domain.Entities.ComplaintDepartmentFeedback>(),
                It.IsAny<ICollection<ComplaintFeedbackAttachment>?>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath();
        var command = new UpdateComplaintDepartmentFeedbackCommand
        {
            Id = 1,
            CorrectiveAction = "Updated fix",
            IsActive = 1
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "COMPLAINT_FEEDBACK_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
