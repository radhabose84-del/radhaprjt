using AutoMapper;
using Contracts.Common;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using ProjectManagement.Domain.Events;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class DeleteMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_DeleteSucceeds_ReturnsSuccess()
        {
            var command = MiscTypeMasterBuilders.ValidDeleteCommand();
            var entity = MiscTypeMasterBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<ProjectManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);

            var sut = CreateSut();
            var result = await sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DeleteFails_ReturnsFailure()
        {
            var command = MiscTypeMasterBuilders.ValidDeleteCommand();
            var entity = MiscTypeMasterBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<ProjectManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(false);

            var sut = CreateSut();
            var result = await sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_DeleteSucceeds_PublishesAuditEvent()
        {
            var command = MiscTypeMasterBuilders.ValidDeleteCommand();
            var entity = MiscTypeMasterBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.MiscTypeMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<ProjectManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);

            var sut = CreateSut();
            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Delete"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
