using AutoMapper;
using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Application.FreightMaster.Commands.CreateFreightMaster;
using LogisticsManagement.Domain.Events;
using LogisticsManagement.UnitTests.TestData;

namespace LogisticsManagement.UnitTests.Application.FreightMaster.Commands
{
    public sealed class CreateFreightMasterCommandHandlerTests
    {
        private readonly Mock<IFreightMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IFreightMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateFreightMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<global::LogisticsManagement.Domain.Entities.FreightMaster>(It.IsAny<CreateFreightMasterCommand>()))
                .Returns(FreightMasterBuilders.ValidEntity(0));

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<global::LogisticsManagement.Domain.Entities.FreightMaster>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(FreightMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("FreightMaster created successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(FreightMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(FreightMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<global::LogisticsManagement.Domain.Entities.FreightMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(FreightMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "FREIGHT_MASTER_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_UsesMapper()
        {
            SetupHappyPath();
            var command = FreightMasterBuilders.ValidCreateCommand();
            await CreateSut().Handle(command, CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<global::LogisticsManagement.Domain.Entities.FreightMaster>(command),
                Times.Once);
        }
    }
}
