using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Application.CustomFields.Commands.DeleteCustomField;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.CustomFields.Commands
{
    public sealed class DeleteCustomFieldCommandHandlerTests
    {
        private readonly Mock<ICustomFieldCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteCustomFieldCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static CustomField ValidEntity(int id = 1) =>
            new() { Id = id };

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            var command = new DeleteCustomFieldCommand { Id = 1 };
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<CustomField>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<CustomField>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var command = new DeleteCustomFieldCommand { Id = 1 };
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<CustomField>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<CustomField>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "custom field"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            var command = new DeleteCustomFieldCommand { Id = 1 };
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<CustomField>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<CustomField>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(command.Id, It.IsAny<CustomField>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsException()
        {
            var command = new DeleteCustomFieldCommand { Id = 99 };
            var entity = ValidEntity(99);

            _mockMapper
                .Setup(m => m.Map<CustomField>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<CustomField>()))
                .ReturnsAsync(false);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not deleted*");
        }
    }
}
