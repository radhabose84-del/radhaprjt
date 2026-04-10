using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.SalesContact.Commands.UpdateSalesContact;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesContact.Commands;

public class UpdateSalesContactCommandHandlerTests
{
    private readonly Mock<ISalesContactCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesContactQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private UpdateSalesContactCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

    private void SetupMapper()
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesContact>(It.IsAny<UpdateSalesContactCommand>()))
            .Returns((UpdateSalesContactCommand cmd) => new SalesManagement.Domain.Entities.SalesContact
            {
                Id = cmd.Id,
                ContactName = cmd.ContactName,
                MobileNumber = cmd.MobileNumber,
                ContactTypeId = cmd.ContactTypeId
            });
    }

    private void SetupUpdateAsync(int returnId = 1)
    {
        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesContact>()))
            .ReturnsAsync(returnId);
    }

    private void SetupPublishAudit()
    {
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_EntityExists_ReturnsSuccess()
    {
        var command = new UpdateSalesContactCommand { Id = 1, ContactName = "Updated", MobileNumber = "9876543210", ContactTypeId = 1, IsActive = 1 };
        SetupMapper();
        SetupUpdateAsync(1);
        SetupPublishAudit();

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EntityExists_CallsUpdateAsync_Once()
    {
        var command = new UpdateSalesContactCommand { Id = 1, ContactName = "Updated", MobileNumber = "9876543210", ContactTypeId = 1, IsActive = 1 };
        SetupMapper();
        SetupUpdateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesContact>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
    {
        var command = new UpdateSalesContactCommand { Id = 1, ContactName = "Updated", MobileNumber = "9876543210", ContactTypeId = 1, IsActive = 1 };
        SetupMapper();
        SetupUpdateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALES_CONTACT_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
