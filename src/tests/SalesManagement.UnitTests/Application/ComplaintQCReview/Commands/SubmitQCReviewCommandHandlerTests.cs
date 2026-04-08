using SalesManagement.Domain.Entities;
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.ComplaintQCReview.Commands.SubmitQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;

using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ComplaintQCReview.Commands;

public sealed class SubmitQCReviewCommandHandlerTests
{
    private readonly Mock<IComplaintQCReviewCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IComplaintQCReviewQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<ITimeZoneService> _mockTzService = new(MockBehavior.Loose);
    private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

    private SubmitQCReviewCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object,
            _mockIpService.Object, _mockTzService.Object, _mockOutbox.Object,
            _mockMediator.Object, _mockMapper.Object);

    private void SetupHappyPath(int newId = 1)
    {
        _mockMapper
            .Setup(m => m.Map<global::SalesManagement.Domain.Entities.ComplaintQCReview>(It.IsAny<SubmitQCReviewCommand>()))
            .Returns(new global::SalesManagement.Domain.Entities.ComplaintQCReview());

        _mockIpService.Setup(s => s.GetUserId()).Returns(1);
        _mockIpService.Setup(s => s.GetUserName()).Returns("test-user");
        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
        _mockTzService.Setup(s => s.GetSystemTimeZone()).Returns("India Standard Time");
        _mockTzService.Setup(s => s.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

        _mockMiscRepo
            .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new global::SalesManagement.Domain.Entities.MiscMaster { Id = 10 });

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<global::SalesManagement.Domain.Entities.ComplaintQCReview>()))
            .ReturnsAsync(newId);

        _mockOutbox
            .Setup(o => o.ScheduleAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review complete",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("QC Review submitted successfully.");
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewId()
    {
        SetupHappyPath(newId: 42);
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);
        result.Data.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateOnce()
    {
        SetupHappyPath();
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<global::SalesManagement.Domain.Entities.ComplaintQCReview>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath();
        var command = new SubmitQCReviewCommand
        {
            ComplaintHeaderId = 1,
            PhysicalVerificationId = 5,
            Comments = "Review",
            ExpectedResolutionDate = new DateOnly(2026, 3, 1)
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionCode == "COMPLAINT_QC_REVIEW_SUBMIT"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
