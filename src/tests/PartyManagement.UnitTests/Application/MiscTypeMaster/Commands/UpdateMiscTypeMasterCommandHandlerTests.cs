using AutoMapper;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class UpdateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync((PartyManagement.Domain.Entities.MiscTypeMaster?)null);

            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(id, It.IsAny<PartyManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<PartyManagement.Domain.Entities.MiscTypeMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingCode_ReturnsFailure()
        {
            var existing = MiscTypeMasterBuilders.ValidEntity(99);
            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(existing);

            var command = MiscTypeMasterBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
