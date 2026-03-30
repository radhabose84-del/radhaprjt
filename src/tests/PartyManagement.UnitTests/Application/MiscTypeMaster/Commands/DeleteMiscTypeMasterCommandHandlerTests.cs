using AutoMapper;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class DeleteMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsSuccess()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity();
            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<PartyManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(MiscTypeMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity();
            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<PartyManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(MiscTypeMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<PartyManagement.Domain.Entities.MiscTypeMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteFailed_ReturnsFailure()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity();
            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<PartyManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(false);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(MiscTypeMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
