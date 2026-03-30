using AutoMapper;
using MediatR;
using UserManagement.Application.Divisions.Commands.CreateDivision;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;

namespace UserManagement.UnitTests.Application.Division.Commands
{
    public sealed class CreateDivisionCommandHandlerTests
    {
        private readonly Mock<IDivisionCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDivisionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private CreateDivisionCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(CreateDivisionCommand command, UserManagement.Domain.Entities.Division createdEntity, DivisionDTO dto)
        {
            _mockQueryRepo
                .Setup(r => r.GetByDivisionnameAsync(command.Name!, null))
                .ReturnsAsync((UserManagement.Domain.Entities.Division?)null);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Division>(command))
                .Returns(createdEntity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Division>()))
                .ReturnsAsync(createdEntity);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockMapper
                .Setup(m => m.Map<DivisionDTO>(createdEntity))
                .Returns(dto);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDivisionDto()
        {
            var command = DivisionBuilders.ValidCreateCommand();
            var entity = DivisionBuilders.ValidEntity();
            var dto = DivisionBuilders.ValidDto();
            SetupHappyPath(command, entity, dto);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.ShortName.Should().Be("DIV01");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = DivisionBuilders.ValidCreateCommand();
            var entity = DivisionBuilders.ValidEntity();
            var dto = DivisionBuilders.ValidDto();
            SetupHappyPath(command, entity, dto);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Division>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = DivisionBuilders.ValidCreateCommand();
            var entity = DivisionBuilders.ValidEntity();
            var dto = DivisionBuilders.ValidDto();
            SetupHappyPath(command, entity, dto);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "Division"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateName_ThrowsValidationException()
        {
            var command = DivisionBuilders.ValidCreateCommand();
            var existingDivision = DivisionBuilders.ValidEntity(id: 5);

            _mockQueryRepo
                .Setup(r => r.GetByDivisionnameAsync(command.Name!, null))
                .ReturnsAsync(existingDivision);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_CreateFails_ThrowsException()
        {
            var command = DivisionBuilders.ValidCreateCommand();
            var entity = DivisionBuilders.ValidEntity(id: 0);
            var dto = DivisionBuilders.ValidDto(id: 0);

            _mockQueryRepo
                .Setup(r => r.GetByDivisionnameAsync(command.Name!, null))
                .ReturnsAsync((UserManagement.Domain.Entities.Division?)null);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Division>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Division>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<DivisionDTO>(entity))
                .Returns(dto);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not created*");
        }
    }
}
