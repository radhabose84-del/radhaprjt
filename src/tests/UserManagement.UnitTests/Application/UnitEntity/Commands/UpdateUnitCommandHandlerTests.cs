using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Units.Commands.UpdateUnit;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.UnitTests.TestData;
using DomainUnit = UserManagement.Domain.Entities.Unit;
using FluentValidation;

namespace UserManagement.UnitTests.Application.UnitEntity.Commands
{
    public sealed class UpdateUnitCommandHandlerTests
    {
        private readonly Mock<IUnitCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UpdateUnitCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private UpdateUnitCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockLogger.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(UpdateUnitCommand command, DomainUnit entity)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.UpdateUnitDto!.Id))
                .ReturnsAsync(UnitEntityBuilders.ValidGetUnitsByIdDto(id: command.UpdateUnitDto!.Id));

            _mockCommandRepo
                .Setup(r => r.ExistsByNameupdateAsync(command.UpdateUnitDto!.UnitName!, command.UpdateUnitDto!.Id))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<DomainUnit>(command.UpdateUnitDto))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateUnitAsync(command.UpdateUnitDto!.Id, It.IsAny<DomainUnit>()))
                .ReturnsAsync(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUnitId()
        {
            var command = UnitEntityBuilders.ValidUpdateCommand();
            var entity = UnitEntityBuilders.ValidEntity();
            SetupHappyPath(command, entity);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = UnitEntityBuilders.ValidUpdateCommand();
            var entity = UnitEntityBuilders.ValidEntity();
            SetupHappyPath(command, entity);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateUnitAsync(command.UpdateUnitDto!.Id, It.IsAny<DomainUnit>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UnitNotFound_ReturnsZero()
        {
            var command = UnitEntityBuilders.ValidUpdateCommand(id: 999);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((GetUnitsByIdDto?)null);

            // Idempotent update: a missing id is a no-op (returns 0), not a thrown exception.
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(0);
        }

        [Fact]
        public async Task Handle_DuplicateUnitName_ThrowsValidationException()
        {
            var command = UnitEntityBuilders.ValidUpdateCommand();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.UpdateUnitDto!.Id))
                .ReturnsAsync(UnitEntityBuilders.ValidGetUnitsByIdDto());

            _mockCommandRepo
                .Setup(r => r.ExistsByNameupdateAsync(command.UpdateUnitDto!.UnitName!, command.UpdateUnitDto!.Id))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_UpdateReturnsMinusOne_ThrowsValidationException()
        {
            var command = UnitEntityBuilders.ValidUpdateCommand();
            var entity = UnitEntityBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.UpdateUnitDto!.Id))
                .ReturnsAsync(UnitEntityBuilders.ValidGetUnitsByIdDto());

            _mockCommandRepo
                .Setup(r => r.ExistsByNameupdateAsync(command.UpdateUnitDto!.UnitName!, command.UpdateUnitDto!.Id))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<DomainUnit>(command.UpdateUnitDto))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateUnitAsync(command.UpdateUnitDto!.Id, It.IsAny<DomainUnit>()))
                .ReturnsAsync(-1);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }
    }
}
