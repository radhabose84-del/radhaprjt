using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Application.Modules.Queries.GetModuleById;
using UserManagement.Application.Modules.Queries.GetModules;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Modules.Queries
{
    public sealed class GetModuleByIdQueryHandlerTests
    {
        private readonly Mock<IModuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetModuleByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.Modules
            {
                Id = 1,
                ModuleName = "Finance",
                IsDeleted = false
            };
            var dto = new ModuleByIdDto { Id = 1, ModuleName = "Finance" };

            _mockQueryRepo
                .Setup(r => r.GetModuleByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<ModuleByIdDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(new GetModuleByIdQuery { Id = 1 }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ModuleName.Should().Be("Finance");
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsValidationException()
        {
            // Arrange
            _mockQueryRepo
                .Setup(r => r.GetModuleByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.Modules?)null);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(
                new GetModuleByIdQuery { Id = 999 }, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.Modules
            {
                Id = 2,
                ModuleName = "HR",
                IsDeleted = false
            };
            var dto = new ModuleByIdDto { Id = 2, ModuleName = "HR" };

            _mockQueryRepo
                .Setup(r => r.GetModuleByIdAsync(2))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<ModuleByIdDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(new GetModuleByIdQuery { Id = 2 }, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.Module == "Modules"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
