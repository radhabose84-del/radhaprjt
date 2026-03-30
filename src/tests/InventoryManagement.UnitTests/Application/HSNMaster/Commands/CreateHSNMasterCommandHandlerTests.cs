using AutoMapper;
using MediatR;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.CreateHSNMaster;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.HSNMaster.Commands
{
    public sealed class CreateHSNMasterCommandHandlerTests
    {
        private readonly Mock<IHSNMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IHSNMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateHSNMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = HSNMasterBuilders.ValidEntity(newId);
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.HSNMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.HSNMaster>()))
                .ReturnsAsync(newId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(
                HSNMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(42);
            var result = await CreateSut().Handle(
                HSNMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(HSNMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.HSNMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(HSNMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create" && e.ActionCode == "HSN_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
