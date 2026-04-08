using SalesManagement.Domain.Entities;
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Commands.UpdateComplaint;
using SalesManagement.Application.Complaint.Dto;

using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.Complaint.Commands;

public sealed class UpdateComplaintCommandHandlerTests
{
    private readonly Mock<IComplaintCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

    private UpdateComplaintCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

    private void SetupHappyPath(int result = 1)
    {
        _mockMapper
            .Setup(m => m.Map<ComplaintHeader>(It.IsAny<UpdateComplaintCommand>()))
            .Returns(new ComplaintHeader { Id = 1 });

        _mockMapper
            .Setup(m => m.Map<ComplaintDetail>(It.IsAny<CreateComplaintDetailDto>()))
            .Returns(new ComplaintDetail());

        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<ComplaintHeader>(), It.IsAny<List<ComplaintDetail>>()))
            .ReturnsAsync(result);

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var command = new UpdateComplaintCommand
        {
            Id = 1,
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            IsActive = 1,
            Details = new List<CreateComplaintDetailDto>()
        };

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Complaint updated successfully.");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUpdateOnce()
    {
        SetupHappyPath();
        var command = new UpdateComplaintCommand
        {
            Id = 1,
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            IsActive = 1,
            Details = new List<CreateComplaintDetailDto>()
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.UpdateAsync(It.IsAny<ComplaintHeader>(), It.IsAny<List<ComplaintDetail>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath();
        var command = new UpdateComplaintCommand
        {
            Id = 1,
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            IsActive = 1,
            Details = new List<CreateComplaintDetailDto>()
        };

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "Update" &&
                    e.ActionCode == "COMPLAINT_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
