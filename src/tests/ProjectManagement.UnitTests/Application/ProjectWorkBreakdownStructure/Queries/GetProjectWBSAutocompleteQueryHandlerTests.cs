using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Autocomplete;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.ProjectWorkBreakdownStructure.Queries
{
    public sealed class GetProjectWBSAutocompleteQueryHandlerTests
    {
        private readonly Mock<IProjectWorkBreakdownStructureQueryRepository> _mockQueryRepo =
            new(MockBehavior.Strict);

        private GetProjectWorkBreakdownStructureAutocompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingItems()
        {
            var list = ProjectWorkBreakdownStructureBuilders.ValidAutocompleteList();
            _mockQueryRepo.Setup(r => r.GetAutocompleteAsync(1, "found"))
                .ReturnsAsync((IReadOnlyList<ProjectWorkBreakdownStructureAutocompleteDto>)list);

            var result = await CreateSut().Handle(
                new GetProjectWorkBreakdownStructureAutocompleteQuery(1, "found"),
                CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NoMatches_ReturnsEmptyList()
        {
            var empty = new List<ProjectWorkBreakdownStructureAutocompleteDto>();
            _mockQueryRepo.Setup(r => r.GetAutocompleteAsync(1, "xyz"))
                .ReturnsAsync((IReadOnlyList<ProjectWorkBreakdownStructureAutocompleteDto>)empty);

            var result = await CreateSut().Handle(
                new GetProjectWorkBreakdownStructureAutocompleteQuery(1, "xyz"),
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NullSearchPattern_ReturnsItems()
        {
            var list = ProjectWorkBreakdownStructureBuilders.ValidAutocompleteList();
            _mockQueryRepo.Setup(r => r.GetAutocompleteAsync(1, null))
                .ReturnsAsync((IReadOnlyList<ProjectWorkBreakdownStructureAutocompleteDto>)list);

            var result = await CreateSut().Handle(
                new GetProjectWorkBreakdownStructureAutocompleteQuery(1, null),
                CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            var empty = new List<ProjectWorkBreakdownStructureAutocompleteDto>();
            _mockQueryRepo.Setup(r => r.GetAutocompleteAsync(2, "test"))
                .ReturnsAsync((IReadOnlyList<ProjectWorkBreakdownStructureAutocompleteDto>)empty);

            await CreateSut().Handle(
                new GetProjectWorkBreakdownStructureAutocompleteQuery(2, "test"),
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAutocompleteAsync(2, "test"), Times.Once);
        }
    }
}
