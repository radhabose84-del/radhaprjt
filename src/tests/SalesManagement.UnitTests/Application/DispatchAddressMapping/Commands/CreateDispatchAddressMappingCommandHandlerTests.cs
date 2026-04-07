using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.CreateDispatchAddressMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.DispatchAddressMapping.Commands;

public class CreateDispatchAddressMappingCommandHandlerTests
{
    private readonly Mock<IDispatchAddressMappingCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IDispatchAddressMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private CreateDispatchAddressMappingCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

    private CreateDispatchAddressMappingCommand ValidCommand() => new()
    {
        PartyId = 1,
        DispatchAddressId = 2,
        UsageTypeId = 3,
        IsDefault = true
    };

    private void SetupMapper(CreateDispatchAddressMappingCommand cmd)
    {
        _mockMapper
            .Setup(m => m.Map<SalesManagement.Domain.Entities.DispatchAddressMapping>(cmd))
            .Returns(new SalesManagement.Domain.Entities.DispatchAddressMapping
            {
                PartyId = cmd.PartyId,
                DispatchAddressId = cmd.DispatchAddressId,
                UsageTypeId = cmd.UsageTypeId,
                IsDefault = cmd.IsDefault
            });
    }

    private void SetupCreateAsync(int returnId = 1)
    {
        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.DispatchAddressMapping>()))
            .ReturnsAsync(returnId);
    }

    private void SetupPublishAudit()
    {
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupCreateAsync(1);
        SetupPublishAudit();

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewEntityId()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupCreateAsync(42);
        SetupPublishAudit();

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.Data.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateAsync_Once()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupCreateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.DispatchAddressMapping>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
    {
        var command = ValidCommand();
        SetupMapper(command);
        SetupCreateAsync(1);
        SetupPublishAudit();

        await CreateSut().Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "DISPATCH_ADDRESS_MAPPING_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
