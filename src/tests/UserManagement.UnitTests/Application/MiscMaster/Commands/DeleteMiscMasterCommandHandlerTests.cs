using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMiscMaster;
using UserManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class DeleteMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id, bool deleteResult = true)
        {
            var entity = new UserManagement.Domain.Entities.MiscMaster { Id = id };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.MiscMaster>(It.IsAny<DeleteMiscMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(id, It.IsAny<UserManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(deleteResult);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupHappyPath(id: 1, deleteResult: true);

            var result = await CreateSut().Handle(new DeleteMiscMasterCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath(id: 1, deleteResult: true);

            await CreateSut().Handle(new DeleteMiscMasterCommand { Id = 1 }, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(1, It.IsAny<UserManagement.Domain.Entities.MiscMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            SetupHappyPath(id: 2, deleteResult: true);

            await CreateSut().Handle(new DeleteMiscMasterCommand { Id = 2 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "MiscMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsException()
        {
            SetupHappyPath(id: 99, deleteResult: false);

            Func<Task> act = async () => await CreateSut().Handle(
                new DeleteMiscMasterCommand { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*deletion failed*");
        }
    }
}
