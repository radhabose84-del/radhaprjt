using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.UpdateDispatchAddressMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.DispatchAddressMapping.Commands;

public class UpdateDispatchAddressMappingCommandHandlerTests
{
    private readonly Mock<IDispatchAddressMappingCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IDispatchAddressMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private UpdateDispatchAddressMappingCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

    private void SetupMapper()
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.DispatchAddressMapping>(It.IsAny<UpdateDispatchAddressMappingCommand>()))
            .Returns((UpdateDispatchAddressMappingCommand cmd) => new SalesManagement.Domain.Entities.DispatchAddressMapping
            {
                Id = cmd.Id,
                IsDefault = cmd.IsDefault
            });
    }

    private void SetupUpdateAsync(int returnId = 1)
    {
        _mockCommandRepo
            .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.DispatchAddressMapping>()))
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
        var command = new UpdateDispatchAddressMappingCommand { Id = 1, IsDefault = true, IsActive = 1 };
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
        var command = new UpdateDispatchAddressMappingCommand { Id = 1, IsDefault = false, IsActive = 1 };
        SetupMapper();
        SetupUpdateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.DispatchAddressMapping>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
    {
        var command = new UpdateDispatchAddressMappingCommand { Id = 1, IsDefault = true, IsActive = 1 };
        SetupMapper();
        SetupUpdateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "DISPATCH_ADDRESS_MAPPING_UPDATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
