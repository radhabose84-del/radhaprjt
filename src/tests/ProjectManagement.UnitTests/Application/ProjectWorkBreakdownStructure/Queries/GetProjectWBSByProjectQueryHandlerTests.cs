using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetByProject;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWBSByProject;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.ProjectWorkBreakdownStructure.Queries
{
    public sealed class GetProjectWBSByProjectQueryHandlerTests
    {
        private readonly Mock<IProjectWorkBreakdownStructureQueryRepository> _mockQueryRepo =
            new(MockBehavior.Strict);

        private GetProjectWorkBreakdownStructureByProjectQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ReturnsListForProject()
        {
            var list = ProjectWorkBreakdownStructureBuilders.ValidDtoList(2);
            _mockQueryRepo.Setup(r => r.GetByProjectAsync(1))
                .ReturnsAsync((IReadOnlyList<ProjectWorkBreakdownStructureDto>)list);

            var result = await CreateSut().Handle(
                new GetProjectWorkBreakdownStructureByProjectQuery(1),
                CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_EmptyProject_ReturnsEmptyList()
        {
            var empty = new List<ProjectWorkBreakdownStructureDto>();
            _mockQueryRepo.Setup(r => r.GetByProjectAsync(99))
                .ReturnsAsync((IReadOnlyList<ProjectWorkBreakdownStructureDto>)empty);

            var result = await CreateSut().Handle(
                new GetProjectWorkBreakdownStructureByProjectQuery(99),
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryGetByProjectOnce()
        {
            var empty = new List<ProjectWorkBreakdownStructureDto>();
            _mockQueryRepo.Setup(r => r.GetByProjectAsync(3))
                .ReturnsAsync((IReadOnlyList<ProjectWorkBreakdownStructureDto>)empty);

            await CreateSut().Handle(
                new GetProjectWorkBreakdownStructureByProjectQuery(3),
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByProjectAsync(3), Times.Once);
        }
    }
}
