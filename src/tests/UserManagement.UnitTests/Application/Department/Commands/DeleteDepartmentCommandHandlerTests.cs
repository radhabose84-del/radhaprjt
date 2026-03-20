using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Departments.Commands.DeleteDepartment;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;
using MaintenanceDeptValidation = Contracts.Interfaces.Lookups.Maintenance.IDepartmentValidationLookup;
using FixedAssetDeptValidation = Contracts.Interfaces.Lookups.FixedAssetManagement.IDepartmentValidationLookup;

namespace UserManagement.UnitTests.Application.Department.Commands
{
    public sealed class DeleteDepartmentCommandHandlerTests
    {
        private readonly Mock<IDepartmentCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDepartmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DeleteDepartmentCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<MaintenanceDeptValidation> _mockMaintenanceLookup = new(MockBehavior.Strict);
        private readonly Mock<FixedAssetDeptValidation> _mockFixedAssetLookup = new(MockBehavior.Strict);

        private DeleteDepartmentCommandHandler CreateSut() =>
            new(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object,
                _mockLogger.Object,
                _mockMaintenanceLookup.Object,
                _mockFixedAssetLookup.Object);

        private void SetupHappyPath(DeleteDepartmentCommand command)
        {
            _mockMaintenanceLookup
                .Setup(l => l.IsDepartmentUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockFixedAssetLookup
                .Setup(l => l.IsDepartmentUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.IsDepartmentUsedByAnyUserAsync(command.Id))
                .ReturnsAsync(false);

            var mappedEntity = new UserManagement.Domain.Entities.Department { Id = command.Id };
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Department>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Department>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveResult()
        {
            var command = DepartmentBuilders.ValidDeleteCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = DepartmentBuilders.ValidDeleteCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "Department"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UsedInMaintenance_ThrowsValidationException()
        {
            var command = DepartmentBuilders.ValidDeleteCommand();

            _mockMaintenanceLookup
                .Setup(l => l.IsDepartmentUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockFixedAssetLookup
                .Setup(l => l.IsDepartmentUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Cannot delete department*");
        }

        [Fact]
        public async Task Handle_UsedByUser_ThrowsValidationException()
        {
            var command = DepartmentBuilders.ValidDeleteCommand();

            _mockMaintenanceLookup
                .Setup(l => l.IsDepartmentUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockFixedAssetLookup
                .Setup(l => l.IsDepartmentUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.IsDepartmentUsedByAnyUserAsync(command.Id))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Cannot delete Department*");
        }
    }
}
