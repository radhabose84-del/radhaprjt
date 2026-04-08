using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Application.Entity.Commands.CreateEntity;
using UserManagement.Application.Entity.Queries.GetEntity;
using UserManagement.Domain.Events;
using Contracts.Common;

namespace UserManagement.UnitTests.Application.Entity.Commands
{
    public sealed class CreateEntityCommandHandlerTests
    {
        private readonly Mock<IEntityCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateEntityCommandHandler>> _mockLogger = new();

        private CreateEntityCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveId()
        {
            var command = new CreateEntityCommand { EntityName = "TestEntity" };
            var entity = new UserManagement.Domain.Entities.Entity { Id = 1, EntityCode = "E001" };

            _mockCommandRepo.Setup(r => r.GetByNameAsync("TestEntity")).ReturnsAsync((UserManagement.Domain.Entities.Entity?)null);
            _mockMediator.Setup(m => m.Send(It.IsAny<IRequest<ApiResponseDTO<string>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<string> { IsSuccess = true, Data = "E001" });
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Entity>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Entity>())).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<GetEntityDTO>(It.IsAny<UserManagement.Domain.Entities.Entity>())).Returns(new GetEntityDTO { Id = 1 });
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_DuplicateName_ThrowsValidationException()
        {
            var command = new CreateEntityCommand { EntityName = "Existing" };
            var existing = new UserManagement.Domain.Entities.Entity { EntityName = "Existing", IsActive = UserManagement.Domain.Enums.Common.Enums.Status.Active };

            _mockCommandRepo.Setup(r => r.GetByNameAsync("Existing")).ReturnsAsync(existing);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        }
    }
}
