using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.Command.CreateMachineSpecfication;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineSpecification.Commands
{
    public sealed class CreateMachineSpecificationCommandHandlerTests
    {
        private readonly Mock<IMachineSpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateMachineSpecficationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateMachineSpecficationCommand ValidCommand() => new()
        {
            Specifications = new List<MachineSpecificationCreateDto>
            {
                new() { SpecificationId = 1, MachineId = 1, SpecificationValue = "Value1" },
                new() { SpecificationId = 2, MachineId = 1, SpecificationValue = "Value2" }
            }
        };

        private void SetupHappyPath()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(It.IsAny<MachineSpecificationCreateDto>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineSpecification());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsIdsForEachSpec()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsyncForEachSpec()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEventForEachSpec()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "MachineSpecification"),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_AllCreateReturnZero_ReturnsNotSuccess()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(It.IsAny<MachineSpecificationCreateDto>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineSpecification());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineSpecification>()))
                .ReturnsAsync(0);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeEmpty();
        }
    }
}
