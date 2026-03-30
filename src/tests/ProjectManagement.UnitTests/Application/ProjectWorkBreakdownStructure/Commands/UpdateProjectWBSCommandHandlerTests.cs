using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.UpdateProjectWorkBreakdownStructureCommand;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Domain.Events;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.ProjectWorkBreakdownStructure.Commands
{
    public sealed class UpdateProjectWBSCommandHandlerTests
    {
        private readonly Mock<IProjectWorkBreakdownStructureCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IProjectWorkBreakdownStructureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateProjectWorkBreakdownStructureCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private UpdateProjectWorkBreakdownStructureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object,
                _mockMediator.Object, _mockLogger.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = ProjectWorkBreakdownStructureBuilders.ValidEntity(id);
            var dto = ProjectWorkBreakdownStructureBuilders.ValidDto(id);

            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure>()))
                .Returns(Task.CompletedTask);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<ProjectWorkBreakdownStructureDto>(It.IsAny<object>()))
                .Returns(dto);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(
                ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand(1), CancellationToken.None);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(
                ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand(1), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(
                ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand(1), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsException()
        {
            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure?)null);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
