using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Commands.UpdateSalesLead;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesLead.Commands;

public class UpdateSalesLeadCommandHandlerTests
{
    private readonly Mock<ISalesLeadCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesLeadQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesContactCommandRepository> _mockContactCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
    private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

    public UpdateSalesLeadCommandHandlerTests()
    {
        // Admin path: no marketing-officer scoping applied
        _mockAccessFilter
            .Setup(f => f.ShouldApplyFilterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private UpdateSalesLeadCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockContactCommandRepo.Object,
            _mockMediator.Object, _mockMapper.Object, _mockAccessFilter.Object);

    private void SetupMapper()
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesLead>(It.IsAny<UpdateSalesLeadCommand>()))
            .Returns((UpdateSalesLeadCommand cmd) => new SalesManagement.Domain.Entities.SalesLead
            {
                Id = cmd.Id,
                ContactName = cmd.ContactName,
                MobileNumber = cmd.MobileNumber,
                MarketingOfficerId = cmd.MarketingOfficerId,
                ContactId = cmd.ContactId
            });
    }

    private void SetupUpdateAsync(int returnId = 1)
    {
        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesLead>()))
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
        var command = new UpdateSalesLeadCommand
        {
            Id = 1, ContactName = "Updated", MobileNumber = "9876543210",
            MarketingOfficerId = 1, ContactId = 5, IsActive = 1,
            InteractionDate = DateTimeOffset.UtcNow
        };
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
        var command = new UpdateSalesLeadCommand
        {
            Id = 1, ContactName = "Updated", MobileNumber = "9876543210",
            MarketingOfficerId = 1, ContactId = 5, IsActive = 1,
            InteractionDate = DateTimeOffset.UtcNow
        };
        SetupMapper();
        SetupUpdateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesLead>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
    {
        var command = new UpdateSalesLeadCommand
        {
            Id = 1, ContactName = "Updated", MobileNumber = "9876543210",
            MarketingOfficerId = 1, ContactId = 5, IsActive = 1,
            InteractionDate = DateTimeOffset.UtcNow
        };
        SetupMapper();
        SetupUpdateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALES_LEAD_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
