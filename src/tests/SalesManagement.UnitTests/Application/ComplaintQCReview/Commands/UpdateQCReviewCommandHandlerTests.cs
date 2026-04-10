using SalesManagement.Domain.Entities;
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.ComplaintQCReview.Commands.UpdateQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;

using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintQCReview.Commands;

public sealed class UpdateQCReviewCommandHandlerTests
{
    private readonly Mock<IComplaintQCReviewCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IComplaintQCReviewQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ITimeZoneService> _mockTzService = new(MockBehavior.Loose);
    private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

    private UpdateQCReviewCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object,
            _mockIpService.Object, _mockTzService.Object, _mockOutbox.Object,
            _mockMediator.Object, _mockMapper.Object);

    private void SetupHappyPath(int result = 1)
    {
        _mockMapper
            .Setup(m => m.Map<global::SalesManagement.Domain.Entities.ComplaintQCReview>(It.IsAny<UpdateQCReviewCommand>()))
            .Returns(new global::SalesManagement.Domain.Entities.ComplaintQCReview { Id = 1 });

        _mockIpService.Setup(s => s.GetUserId()).Returns(1);
        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
        _mockTzService.Setup(s => s.GetSystemTimeZone()).Returns("India Standard Time");
        _mockTzService.Setup(s => s.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new global::SalesManagement.Domain.Entities.MiscMaster { Id = 10 });

        _mockCommandRepo
            .Setup(r => r.UpdateAsync(
                It.IsAny<global::SalesManagement.Domain.Entities.ComplaintQCReview>(),
                It.IsAny<List<ComplaintQCReviewAssignment>>()))
            .ReturnsAsync(result);

        _mockQueryRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new ComplaintQCReviewDto { Id = 1, ComplaintHeaderId = 10 });

        _mockOutbox
            .Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var command = new UpdateQCReviewCommand
        {
            Id = 1,
            PhysicalVerificationId = 5,
            Comments = "Updated review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1),
            IsActive = 1
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("QC Review updated successfully.");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUpdateOnce()
    {
        SetupHappyPath();
        var command = new UpdateQCReviewCommand
        {
            Id = 1,
            PhysicalVerificationId = 5,
            Comments = "Updated",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1),
            IsActive = 1
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.UpdateAsync(
                It.IsAny<global::SalesManagement.Domain.Entities.ComplaintQCReview>(),
                It.IsAny<List<ComplaintQCReviewAssignment>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath();
        var command = new UpdateQCReviewCommand
        {
            Id = 1,
            PhysicalVerificationId = 5,
            Comments = "Updated",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1),
            IsActive = 1
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "COMPLAINT_QC_REVIEW_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
