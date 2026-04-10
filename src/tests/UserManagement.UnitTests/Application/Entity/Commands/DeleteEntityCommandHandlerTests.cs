using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Application.Entity.Commands.DeleteEntity;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Entity.Commands
{
    public sealed class DeleteEntityCommandHandlerTests
    {
        private readonly Mock<IEntityCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IEntityQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILogger<DeleteEntityCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteEntityCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockLogger.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ExistingEntity_ReturnsPositiveResult()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new UserManagement.Domain.Entities.Entity { Id = 1 });

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Entity>(It.IsAny<DeleteEntityCommand>()))
                .Returns(new UserManagement.Domain.Entities.Entity());

            _mockCommandRepo
                .Setup(r => r.DeleteEntityAsync(1, It.IsAny<UserManagement.Domain.Entities.Entity>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new DeleteEntityCommand { EntityId = 1 }, CancellationToken.None);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.Entity?)null!);

            Func<Task> act = () => CreateSut().Handle(
                new DeleteEntityCommand { EntityId = 999 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
