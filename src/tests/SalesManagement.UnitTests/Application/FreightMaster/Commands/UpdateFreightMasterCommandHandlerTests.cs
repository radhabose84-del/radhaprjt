using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IFreightMaster;
using SalesManagement.Application.FreightMaster.Commands.UpdateFreightMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.FreightMaster.Commands
{
    public class UpdateFreightMasterCommandHandlerTests
    {
        private readonly Mock<IFreightMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IFreightMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateFreightMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateFreightMasterCommand ValidCommand() => new()
        {
            Id = 1,
            FreightModeId = 1,
            RateMethodId = 2,
            Rate = 200m,
            IsActive = 1
        };

        private void SetupMapper()
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.FreightMaster>(It.IsAny<UpdateFreightMasterCommand>()))
                .Returns((UpdateFreightMasterCommand cmd) => new SalesManagement.Domain.Entities.FreightMaster
                {
                    Id = cmd.Id,
                    FreightModeId = cmd.FreightModeId,
                    RateMethodId = cmd.RateMethodId,
                    Rate = cmd.Rate
                });
        }

        private void SetupUpdateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.FreightMaster>()))
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
        public async Task Handle_EntityExists_ReturnsSuccessMessage()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_EntityExists_CallsUpdateAsync_Once()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.FreightMaster>()),
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
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "FREIGHT_MASTER_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_AuditEvent_ContainsEntityId()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "1"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
