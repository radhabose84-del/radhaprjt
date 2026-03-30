using AutoMapper;
using UserManagement.Application.Common.Interfaces.ILanguage;
using UserManagement.Application.Language.Commands.CreateLanguage;
using UserManagement.Application.Language.Queries.GetLanguages;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using MediatR;
using FluentValidation;

namespace UserManagement.UnitTests.Application.Language.Commands
{
    public class CreateLanguageCommandHandlerTests
    {
        private readonly Mock<ILanguageCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILanguageQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateLanguageCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsLanguageDto()
        {
            var command = LanguageBuilders.ValidCreateCommand();
            var entity = LanguageBuilders.ValidEntity(id: 1);
            var dto = LanguageBuilders.ValidDto(id: 1);

            _mockQueryRepo
                .Setup(r => r.GetByLanguagenameAsync(command.Name, null))
                .ReturnsAsync((UserManagement.Domain.Entities.Language?)null);
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Language>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Language>()))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map<LanguageDTO>(entity))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("English");
        }

        [Fact]
        public async Task Handle_DuplicateLanguage_ThrowsValidationException()
        {
            var command = LanguageBuilders.ValidCreateCommand();
            var existingLanguage = LanguageBuilders.ValidEntity(id: 5);

            _mockQueryRepo
                .Setup(r => r.GetByLanguagenameAsync(command.Name, null))
                .ReturnsAsync(existingLanguage);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = LanguageBuilders.ValidCreateCommand();
            var entity = LanguageBuilders.ValidEntity(id: 1);
            var dto = LanguageBuilders.ValidDto(id: 1);

            _mockQueryRepo
                .Setup(r => r.GetByLanguagenameAsync(command.Name, null))
                .ReturnsAsync((UserManagement.Domain.Entities.Language?)null);
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Language>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Language>()))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map<LanguageDTO>(entity))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "Language"),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create" && e.Module == "Language"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = LanguageBuilders.ValidCreateCommand();
            var entity = LanguageBuilders.ValidEntity(id: 1);
            var dto = LanguageBuilders.ValidDto(id: 1);

            _mockQueryRepo
                .Setup(r => r.GetByLanguagenameAsync(command.Name, null))
                .ReturnsAsync((UserManagement.Domain.Entities.Language?)null);
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Language>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Language>()))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map<LanguageDTO>(entity))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await sut.Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Language>()),
                Times.Once);
        }
    }
}
