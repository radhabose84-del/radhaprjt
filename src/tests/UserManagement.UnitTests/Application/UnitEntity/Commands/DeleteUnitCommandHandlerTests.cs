using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Units.Commands.DeleteUnit;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Application.Common.Interfaces.IUnit;
using DomainUnit = UserManagement.Domain.Entities.Unit;
using UserManagement.UnitTests.TestData;
using FluentValidation;

namespace UserManagement.UnitTests.Application.UnitEntity.Commands
{
    public sealed class DeleteUnitCommandHandlerTests
    {
        private readonly Mock<IUnitCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DeleteUnitCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private DeleteUnitCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockLogger.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(DeleteUnitCommand command, DomainUnit entity)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.UnitId))
                .ReturnsAsync(UnitEntityBuilders.ValidGetUnitsByIdDto(id: command.UnitId));

            _mockQueryRepo
                .Setup(r => r.IsUnitUsedByAnyUserAsync(command.UnitId))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<DomainUnit>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteUnitAsync(command.UnitId, It.IsAny<DomainUnit>()))
                .ReturnsAsync(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUnitId()
        {
            var command = UnitEntityBuilders.ValidDeleteCommand();
            var entity = UnitEntityBuilders.ValidEntity();
            SetupHappyPath(command, entity);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task Handle_UnitNotFound_ReturnsZero()
        {
            var command = UnitEntityBuilders.ValidDeleteCommand(unitId: 999);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((GetUnitsByIdDto?)null);

            // Idempotent delete: a missing id is a no-op (returns 0), not a thrown exception.
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(0);
        }

        [Fact]
        public async Task Handle_UnitUsedByUser_ThrowsValidationException()
        {
            var command = UnitEntityBuilders.ValidDeleteCommand();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.UnitId))
                .ReturnsAsync(UnitEntityBuilders.ValidGetUnitsByIdDto());

            _mockQueryRepo
                .Setup(r => r.IsUnitUsedByAnyUserAsync(command.UnitId))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Cannot delete*");
        }

        [Fact]
        public async Task Handle_DeleteReturnsMinusOne_ThrowsValidationException()
        {
            var command = UnitEntityBuilders.ValidDeleteCommand();
            var entity = UnitEntityBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.UnitId))
                .ReturnsAsync(UnitEntityBuilders.ValidGetUnitsByIdDto());

            _mockQueryRepo
                .Setup(r => r.IsUnitUsedByAnyUserAsync(command.UnitId))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<DomainUnit>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteUnitAsync(command.UnitId, It.IsAny<DomainUnit>()))
                .ReturnsAsync(-1);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }
    }
}
