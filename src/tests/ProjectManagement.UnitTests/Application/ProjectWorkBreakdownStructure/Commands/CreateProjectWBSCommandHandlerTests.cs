using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.CreateProjectWorkBreakdownStructureCommand;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Domain.Events;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.ProjectWorkBreakdownStructure.Commands
{
    public sealed class CreateProjectWBSCommandHandlerTests
    {
        private readonly Mock<IProjectWorkBreakdownStructureCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IProjectWorkBreakdownStructureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateProjectWorkBreakdownStructureCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreateProjectWorkBreakdownStructureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object,
                _mockMediator.Object, _mockLogger.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = ProjectWorkBreakdownStructureBuilders.ValidEntity(newId);
            var dto = ProjectWorkBreakdownStructureBuilders.ValidDto(newId);

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure>(It.IsAny<object>()))
                .Returns(entity);

            _mockMapper
                .Setup(m => m.Map<ProjectWorkBreakdownStructureDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockCommandRepo
                .Setup(r => r.AddAsync(It.IsAny<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(newId))
                .ReturnsAsync(dto);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(
                ProjectWorkBreakdownStructureBuilders.ValidCreateCommand(), CancellationToken.None);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAddOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(
                ProjectWorkBreakdownStructureBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.AddAsync(It.IsAny<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(
                ProjectWorkBreakdownStructureBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithParentId_QueriesParentLevel()
        {
            var entity = ProjectWorkBreakdownStructureBuilders.ValidEntity(1);
            var dto = ProjectWorkBreakdownStructureBuilders.ValidDto(1);

            _mockMapper.Setup(m => m.Map<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure>(It.IsAny<object>())).Returns(entity);
            _mockMapper.Setup(m => m.Map<ProjectWorkBreakdownStructureDto>(It.IsAny<object>())).Returns(dto);
            _mockCommandRepo.Setup(r => r.AddAsync(It.IsAny<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure>())).ReturnsAsync(entity);
            _mockQueryRepo.Setup(r => r.GetParentLevelAsync(5)).ReturnsAsync(1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var command = ProjectWorkBreakdownStructureBuilders.ValidCreateCommand();
            command.ParentWorkBreakdownScheduleIIIMasterId = 5;

            await CreateSut().Handle(command, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetParentLevelAsync(5), Times.Once);
        }
    }
}
