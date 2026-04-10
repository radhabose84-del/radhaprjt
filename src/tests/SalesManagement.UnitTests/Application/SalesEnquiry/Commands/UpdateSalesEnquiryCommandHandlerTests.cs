using SalesManagement.Domain.Entities;
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.UpdateSalesEnquiry;

using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesEnquiry.Commands;

public sealed class UpdateSalesEnquiryCommandHandlerTests
{
    private readonly Mock<ISalesEnquiryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesEnquiryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private UpdateSalesEnquiryCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    private void SetupHappyPath(int result = 1)
    {
        _mockMapper
            .Setup(m => m.Map<SalesEnquiryHeader>(It.IsAny<object>()))
            .Returns(new SalesEnquiryHeader { Id = 1, PartyId = 1 });

        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<SalesEnquiryHeader>()))
            .ReturnsAsync(result);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsResult()
    {
        SetupHappyPath(1);
        var command = new UpdateSalesEnquiryCommand { Id = 1, PartyId = 1, IsActive = 1 };
        var result = await CreateSut().Handle(command, CancellationToken.None);
        result.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUpdateOnce()
    {
        SetupHappyPath(1);
        var command = new UpdateSalesEnquiryCommand { Id = 1, PartyId = 1, IsActive = 1 };
        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.UpdateAsync(It.IsAny<SalesEnquiryHeader>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath(1);
        var command = new UpdateSalesEnquiryCommand { Id = 1, PartyId = 1, IsActive = 1 };
        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "Update" &&
                    e.ActionCode == "SALESENQUIRY_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
