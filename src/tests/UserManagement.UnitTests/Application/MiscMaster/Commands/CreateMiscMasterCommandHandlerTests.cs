using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMiscMaster;
using UserManagement.Application.MiscMaster.Command.CreateMiscMaster;
using UserManagement.Application.MiscMaster.Queries.GetMiscMaster;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class CreateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(CreateMiscMasterCommand command, int newId = 1)
        {
            var entity = new UserManagement.Domain.Entities.MiscMaster
            {
                Id = newId,
                MiscTypeId = command.MiscTypeId,
                Code = command.Code,
                Description = command.Description
            };

            var dto = new GetMiscMasterDto { Id = newId, Code = command.Code, Description = command.Description };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.MiscMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(newId))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            var command = new CreateMiscMasterCommand { MiscTypeId = 1, Code = "MC001", Description = "Test Misc" };
            SetupHappyPath(command, newId: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCorrectCode()
        {
            var command = new CreateMiscMasterCommand { MiscTypeId = 1, Code = "MC001", Description = "Test Misc" };
            SetupHappyPath(command, newId: 3);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Code.Should().Be("MC001");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = new CreateMiscMasterCommand { MiscTypeId = 1, Code = "MC001", Description = "Test Misc" };
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.MiscMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = new CreateMiscMasterCommand { MiscTypeId = 1, Code = "MC001", Description = "Test Misc" };
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "MiscMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateFails_ThrowsException()
        {
            var command = new CreateMiscMasterCommand { MiscTypeId = 1, Code = "MC001", Description = "Test Misc" };
            var entity = new UserManagement.Domain.Entities.MiscMaster { Id = 0, Code = command.Code };
            var dto = new GetMiscMasterDto { Id = 0 };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.MiscMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(0))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Failed to create Misc  Master*");
        }
    }
}
