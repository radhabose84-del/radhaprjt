using AutoMapper;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IMiscMaster;
using PartyManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class DeleteMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            var entity = MiscMasterBuilders.ValidEntity();
            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.MiscMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<PartyManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(MiscMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            var entity = MiscMasterBuilders.ValidEntity();
            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.MiscMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<PartyManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(MiscMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<PartyManagement.Domain.Entities.MiscMaster>()), Times.Once);
        }
    }
}
