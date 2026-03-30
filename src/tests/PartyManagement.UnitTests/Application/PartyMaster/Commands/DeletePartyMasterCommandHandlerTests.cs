using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Command.DeletePartyMaster;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;
using Xunit;

namespace PartyManagement.UnitTests.Application.PartyMaster.Commands
{
    public sealed class DeletePartyMasterCommandHandlerTests
    {
        private readonly Mock<IPartyMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IPartyActivityLogCommandRepository> _mockActivityLog = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private DeletePartyMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockActivityLog.Object, _mockIp.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = PartyMasterBuilders.ValidEntity(id);

            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.PartyMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(id, It.IsAny<PartyManagement.Domain.Entities.PartyMaster>()))
                .ReturnsAsync(true);

            _mockActivityLog
                .Setup(r => r.InsertAsync(It.IsAny<PartyManagement.Domain.Entities.PartyActivityLog>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(PartyMasterBuilders.ValidDeleteCommand(1), CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(PartyMasterBuilders.ValidDeleteCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(1, It.IsAny<PartyManagement.Domain.Entities.PartyMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(PartyMasterBuilders.ValidDeleteCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsExceptionRules()
        {
            var entity = PartyMasterBuilders.ValidEntity(99);

            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.PartyMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(99, It.IsAny<PartyManagement.Domain.Entities.PartyMaster>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(
                PartyMasterBuilders.ValidDeleteCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*deletion failed*");
        }
    }
}
