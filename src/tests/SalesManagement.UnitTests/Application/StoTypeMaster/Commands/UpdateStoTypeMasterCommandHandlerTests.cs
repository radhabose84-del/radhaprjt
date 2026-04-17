using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Commands.UpdateStoTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.StoTypeMaster.Commands;

public class UpdateStoTypeMasterCommandHandlerTests
{
    private readonly Mock<IStoTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IStoTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private UpdateStoTypeMasterCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

    private void SetupMapper()
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.StoTypeMaster>(It.IsAny<UpdateStoTypeMasterCommand>()))
            .Returns((UpdateStoTypeMasterCommand cmd) => new SalesManagement.Domain.Entities.StoTypeMaster
            {
                Id = cmd.Id,
                StoTypeName = cmd.StoTypeName,
                Description = cmd.Description,
                PgiMovementTypeId = cmd.PgiMovementTypeId,
                GrMovementTypeId = cmd.GrMovementTypeId
            });
    }

    private void SetupUpdateAsync(int returnId = 1)
    {
        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.StoTypeMaster>()))
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
        var command = new UpdateStoTypeMasterCommand { Id = 1, StoTypeName = "Updated", PgiMovementTypeId = 1, GrMovementTypeId = 2, IsActive = 1 };
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
        var command = new UpdateStoTypeMasterCommand { Id = 1, StoTypeName = "Updated", PgiMovementTypeId = 1, GrMovementTypeId = 2, IsActive = 1 };
        SetupMapper();
        SetupUpdateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.StoTypeMaster>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
    {
        var command = new UpdateStoTypeMasterCommand { Id = 1, StoTypeName = "Updated", PgiMovementTypeId = 1, GrMovementTypeId = 2, IsActive = 1 };
        SetupMapper();
        SetupUpdateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "STO_TYPE_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
