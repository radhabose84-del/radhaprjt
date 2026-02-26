using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Application.MiscTypeMaster.Commands
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
                .Setup(m => m.Map<SalesManagement.Domain.Entities.MiscTypeMaster>(command))
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
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

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
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

            result.Data.Should().Be(expectedId);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: "MISC001");
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

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
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: "MISC001");
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "MISC001"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity_Once()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<SalesManagement.Domain.Entities.MiscTypeMaster>(command),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsEntityStatusActive()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SalesManagement.Domain.Entities.MiscTypeMaster? capturedEntity = null;

            var entity = new SalesManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = command.MiscTypeCode,
                Description = command.Description
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.MiscTypeMaster>()))
                .Callback<SalesManagement.Domain.Entities.MiscTypeMaster>(e => capturedEntity = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsIsDeletedNotDeleted()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SalesManagement.Domain.Entities.MiscTypeMaster? capturedEntity = null;

            var entity = new SalesManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = command.MiscTypeCode,
                Description = command.Description
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.MiscTypeMaster>()))
                .Callback<SalesManagement.Domain.Entities.MiscTypeMaster>(e => capturedEntity = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }
    }
}
