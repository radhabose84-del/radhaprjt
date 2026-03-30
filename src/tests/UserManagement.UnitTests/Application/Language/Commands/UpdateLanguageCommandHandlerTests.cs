using AutoMapper;
using UserManagement.Application.Common.Interfaces.ILanguage;
using UserManagement.Application.Language.Commands.UpdateLanguage;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using MediatR;
using FluentValidation;

namespace UserManagement.UnitTests.Application.Language.Commands
{
    public class UpdateLanguageCommandHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<ILanguageCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILanguageQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateLanguageCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = LanguageBuilders.ValidUpdateCommand();
            var entity = LanguageBuilders.ValidEntity(id: command.Id);

            _mockQueryRepo
                .Setup(r => r.GetByLanguagenameAsync(command.Name, command.Id))
                .ReturnsAsync((UserManagement.Domain.Entities.Language?)null);
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Language>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<UserManagement.Domain.Entities.Language>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DuplicateName_ThrowsValidationException()
        {
            var command = LanguageBuilders.ValidUpdateCommand(name: "Hindi");
            var existingLanguage = LanguageBuilders.ValidEntity(id: 99, name: "Hindi");

            _mockQueryRepo
                .Setup(r => r.GetByLanguagenameAsync("Hindi", command.Id))
                .ReturnsAsync(existingLanguage);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_UpdateFails_ThrowsException()
        {
            var command = LanguageBuilders.ValidUpdateCommand();
            var entity = LanguageBuilders.ValidEntity(id: command.Id);

            _mockQueryRepo
                .Setup(r => r.GetByLanguagenameAsync(command.Name, command.Id))
                .ReturnsAsync((UserManagement.Domain.Entities.Language?)null);
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Language>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<UserManagement.Domain.Entities.Language>()))
                .ReturnsAsync(false);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not updated*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = LanguageBuilders.ValidUpdateCommand();
            var entity = LanguageBuilders.ValidEntity(id: command.Id);

            _mockQueryRepo
                .Setup(r => r.GetByLanguagenameAsync(command.Name, command.Id))
                .ReturnsAsync((UserManagement.Domain.Entities.Language?)null);
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Language>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<UserManagement.Domain.Entities.Language>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.Module == "Language"),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
