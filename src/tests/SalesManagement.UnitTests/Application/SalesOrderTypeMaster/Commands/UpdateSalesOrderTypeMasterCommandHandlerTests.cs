using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.UpdateSalesOrderTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrderTypeMaster.Commands;

public class UpdateSalesOrderTypeMasterCommandHandlerTests
{
    private readonly Mock<ISalesOrderTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private UpdateSalesOrderTypeMasterCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

    private static UpdateSalesOrderTypeMasterCommand ValidCommand() => new()
    {
        Id = 1,
        TypeName = "Updated Sales Order Type",
        Description = "Updated description",
        IsActive = 1
    };

    private void SetupMapper()
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOrderTypeMaster>(It.IsAny<UpdateSalesOrderTypeMasterCommand>()))
            .Returns((UpdateSalesOrderTypeMasterCommand cmd) => new SalesManagement.Domain.Entities.SalesOrderTypeMaster
            {
                Id = cmd.Id,
                TypeName = cmd.TypeName,
                Description = cmd.Description
            });
    }

    private void SetupUpdateAsync(int returnId = 1)
    {
        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesOrderTypeMaster>()))
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
        SetupMapper();
        SetupUpdateAsync(1);
        SetupPublishAudit();

        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EntityExists_ReturnsUpdatedId()
    {
        SetupMapper();
        SetupUpdateAsync(7);
        SetupPublishAudit();

        var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        result.Data.Should().Be(7);
    }

    [Fact]
    public async Task Handle_EntityExists_CallsUpdateAsync_Once()
    {
        SetupMapper();
        SetupUpdateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesOrderTypeMaster>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
    {
        SetupMapper();
        SetupUpdateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(ValidCommand(), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALESORDERTYPEMASTER_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
