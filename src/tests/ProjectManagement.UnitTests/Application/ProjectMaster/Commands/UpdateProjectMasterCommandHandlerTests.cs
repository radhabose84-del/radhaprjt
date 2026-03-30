using AutoMapper;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Application.ProjectMaster.Command.UpdateProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.ProjectMaster.Commands
{
    public sealed class UpdateProjectMasterCommandHandlerTests
    {
        private readonly Mock<IProjectMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IProjectMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateProjectMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = ProjectMasterBuilders.ValidEntity(id);
            var dto = new ProjectMasterDto { Id = id, ProjectCode = "PRJ001" };

            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<ProjectManagement.Domain.Entities.ProjectMaster>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ProjectMasterBuilders.ValidDto(id));

            _mockMapper
                .Setup(m => m.Map<ProjectMasterDto>(It.IsAny<object>()))
                .Returns(dto);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(ProjectMasterBuilders.ValidUpdateCommand(1), CancellationToken.None);
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(ProjectMasterBuilders.ValidUpdateCommand(1), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<ProjectManagement.Domain.Entities.ProjectMaster>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsException()
        {
            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProjectManagement.Domain.Entities.ProjectMaster?)null);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                ProjectMasterBuilders.ValidUpdateCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
