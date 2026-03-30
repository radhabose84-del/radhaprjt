using AutoMapper;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IMiscMaster;
using PartyManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class UpdateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = MiscMasterBuilders.ValidEntity(id);

            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.MiscMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(id, It.IsAny<PartyManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var command = MiscMasterBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            var command = MiscMasterBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<PartyManagement.Domain.Entities.MiscMaster>()), Times.Once);
        }
    }
}
