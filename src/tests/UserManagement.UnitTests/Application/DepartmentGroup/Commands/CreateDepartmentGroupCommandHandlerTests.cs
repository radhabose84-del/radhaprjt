using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Command.CreateDepartmentGroup;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.DepartmentGroup.Commands
{
    public sealed class CreateDepartmentGroupCommandHandlerTests
    {
        private readonly Mock<IDepartmentGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateDepartmentGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static CreateDepartmentGroupCommand ValidCommand() =>
            new()
            {
                DepartmentGroupCode = "DG001",
                DepartmentGroupName = "Test Department Group",
                IsActive = 1
            };

        private static UserManagement.Domain.Entities.DepartmentGroup ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                DepartmentGroupCode = "DG001",
                DepartmentGroupName = "Test Department Group"
            };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = ValidCommand();
            var entity = ValidEntity(1);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.DepartmentGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.DepartmentGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.DepartmentGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "DepartmentGroup"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZeroOrNegative_ThrowsException()
        {
            var command = ValidCommand();
            var entity = ValidEntity();

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.DepartmentGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.DepartmentGroup>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*creation failed*");
        }
    }
}
