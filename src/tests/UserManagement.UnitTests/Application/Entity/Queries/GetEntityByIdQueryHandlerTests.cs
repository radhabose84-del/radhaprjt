using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Application.Entity.Queries.GetEntity;
using UserManagement.Application.Entity.Queries.GetEntityById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Entity.Queries
{
    public sealed class GetEntityByIdQueryHandlerTests
    {
        private readonly Mock<IEntityQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetEntityByIdQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetEntityByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ExistingEntity_ReturnsMappedDto()
        {
            var entity = new UserManagement.Domain.Entities.Entity { Id = 1, EntityName = "Test" };
            var dto = new GetEntityDTO { Id = 1, EntityName = "Test" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetEntityDTO>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetEntityByIdQuery { EntityId = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.Entity?)null!);

            Func<Task> act = () => CreateSut().Handle(
                new GetEntityByIdQuery { EntityId = 999 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
