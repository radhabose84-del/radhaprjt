using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IMiscTypeMaster;
using QCManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using QCManagement.Domain.Events;
using QCManagement.UnitTests.TestData;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public class CreateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateMiscTypeMasterCommandHandler CreateSut() =>
            new CreateMiscTypeMasterCommandHandler(
                _mockCommandRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        private void SetupHappyPath(CreateMiscTypeMasterCommand command, int newId = 1)
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<QCManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);

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
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewEntityId()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            const int expectedId = 42;
            SetupHappyPath(command, newId: expectedId);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(expectedId);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: "QP_GROUP");
            SetupHappyPath(command, newId: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "MISC_TYPE_CREATE" &&
                        e.Module == "MiscTypeMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditEvent_ContainsMiscTypeCode()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: "QP_GROUP");
            SetupHappyPath(command, newId: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "QP_GROUP"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity_Once()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<QCManagement.Domain.Entities.MiscTypeMaster>(command),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsEntityStatusActive()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            QCManagement.Domain.Entities.MiscTypeMaster? capturedEntity = null;

            var entity = new QCManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = command.MiscTypeCode,
                Description = command.Description
            };

            _mockMapper
                .Setup(m => m.Map<QCManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<QCManagement.Domain.Entities.MiscTypeMaster>()))
                .Callback<QCManagement.Domain.Entities.MiscTypeMaster>(e => capturedEntity = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.IsActive.Should().Be(Status.Active);
        }
    }
}
