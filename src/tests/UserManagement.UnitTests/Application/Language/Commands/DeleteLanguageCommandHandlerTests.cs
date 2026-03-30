using AutoMapper;
using UserManagement.Application.Common.Interfaces.ILanguage;
using UserManagement.Application.Language.Commands.DeleteLanguage;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using MediatR;

namespace UserManagement.UnitTests.Application.Language.Commands
{
    public class DeleteLanguageCommandHandlerTests
    {
        private readonly Mock<ILanguageCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILanguageQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private DeleteLanguageCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = LanguageBuilders.ValidDeleteCommand(id: 1);
            var entity = new UserManagement.Domain.Entities.Language { Id = 1 };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Language>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<UserManagement.Domain.Entities.Language>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsException()
        {
            var command = LanguageBuilders.ValidDeleteCommand(id: 999);
            var entity = new UserManagement.Domain.Entities.Language { Id = 999 };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Language>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(999, It.IsAny<UserManagement.Domain.Entities.Language>()))
                .ReturnsAsync(false);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not deleted*");
        }

        [Fact]
        public async Task Handle_ValidDelete_PublishesAuditEvent()
        {
            var command = LanguageBuilders.ValidDeleteCommand(id: 1);
            var entity = new UserManagement.Domain.Entities.Language { Id = 1 };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Language>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<UserManagement.Domain.Entities.Language>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "Language"),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

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
