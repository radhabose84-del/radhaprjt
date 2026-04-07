using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IFreightMaster;
using SalesManagement.Application.FreightMaster.Commands.CreateFreightMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.FreightMaster.Commands
{
    public class CreateFreightMasterCommandHandlerTests
    {
        private readonly Mock<IFreightMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IFreightMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateFreightMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateFreightMasterCommand ValidCommand() => new()
        {
            FreightModeId = 1,
            RateMethodId = 2,
            Rate = 150.50m
        };

        private void SetupMapper(CreateFreightMasterCommand cmd)
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.FreightMaster>(cmd))
                .Returns(new SalesManagement.Domain.Entities.FreightMaster
                {
                    FreightModeId = cmd.FreightModeId,
                    RateMethodId = cmd.RateMethodId,
                    Rate = cmd.Rate
                });
        }

        private void SetupCreateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.FreightMaster>()))
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
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.FreightMaster>()),
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
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "FREIGHT_MASTER_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
