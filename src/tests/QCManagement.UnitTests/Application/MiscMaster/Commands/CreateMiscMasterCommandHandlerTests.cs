using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IMiscMaster;
using QCManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using QCManagement.Domain.Events;
using QCManagement.UnitTests.TestData;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.UnitTests.Application.MiscMaster.Commands
{
    public class CreateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateMiscMasterCommandHandler CreateSut() =>
            new CreateMiscMasterCommandHandler(
                _mockCommandRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        private void SetupHappyPath(CreateMiscMasterCommand command, int newId = 1, int maxSortOrder = 0)
        {
            var entity = MiscMasterBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<QCManagement.Domain.Entities.MiscMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.GetMaxSortOrderAsync(command.MiscTypeId))
                .ReturnsAsync(maxSortOrder);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(
                    It.IsAny<AuditLogsDomainEvent>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewEntityId()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            const int expectedId = 42;
            SetupHappyPath(command, newId: expectedId);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(expectedId);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.MiscMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: "PHY");
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "MISC_MASTER_CREATE" &&
                        e.Module == "MiscMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ComputesNextSortOrder()
        {
            var command = MiscMasterBuilders.ValidCreateCommand(miscTypeId: 1);
            QCManagement.Domain.Entities.MiscMaster? capturedEntity = null;

            var entity = new QCManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = command.MiscTypeId,
                Code = command.Code,
                Description = command.Description
            };

            _mockMapper.Setup(m => m.Map<QCManagement.Domain.Entities.MiscMaster>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.GetMaxSortOrderAsync(1)).ReturnsAsync(5);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.MiscMaster>()))
                .Callback<QCManagement.Domain.Entities.MiscMaster>(e => capturedEntity = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.SortOrder.Should().Be(6);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity_Once()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<QCManagement.Domain.Entities.MiscMaster>(command),
                Times.Once);
        }
    }
}
