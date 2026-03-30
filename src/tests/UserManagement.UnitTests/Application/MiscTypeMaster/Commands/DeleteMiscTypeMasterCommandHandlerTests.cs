using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class DeleteMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id, bool deleteResult = true)
        {
            var entity = new UserManagement.Domain.Entities.MiscTypeMaster { Id = id };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<DeleteMiscTypeMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(id, It.IsAny<UserManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(deleteResult);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(id: 1, deleteResult: true);

            var result = await CreateSut().Handle(new DeleteMiscTypeMasterCommand { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteOnce()
        {
            SetupHappyPath(id: 1, deleteResult: true);

            await CreateSut().Handle(new DeleteMiscTypeMasterCommand { Id = 1 }, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(1, It.IsAny<UserManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(id: 2, deleteResult: true);

            await CreateSut().Handle(new DeleteMiscTypeMasterCommand { Id = 2 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "MiscTypeMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteFails_ReturnsFailureResponse()
        {
            SetupHappyPath(id: 99, deleteResult: false);

            var result = await CreateSut().Handle(new DeleteMiscTypeMasterCommand { Id = 99 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
