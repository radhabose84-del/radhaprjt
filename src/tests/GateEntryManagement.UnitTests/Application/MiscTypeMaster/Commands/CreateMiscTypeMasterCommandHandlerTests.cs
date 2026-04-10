using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using GateEntryManagement.Domain.Events;
using GateEntryManagement.UnitTests.TestData;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class CreateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<GateEntryManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<CreateMiscTypeMasterCommand>()))
                .Returns(MiscTypeMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<GateEntryManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupHappyPath();
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Misc Type Master created successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupHappyPath(newId: 42);
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<GateEntryManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "MISC_TYPE_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
