using Contracts.Interfaces;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWbsLookup;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.ProjectWorkBreakdownStructure.Queries
{
    public sealed class GetProjectWbsLookupQueryHandlerTests
    {
        private readonly Mock<IProjectWorkBreakdownStructureQueryRepository> _mockQueryRepo =
            new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetProjectWbsLookupQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var list = ProjectWorkBreakdownStructureBuilders.ValidLookupList();
            _mockIpService.Setup(x => x.GetUnitId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetWbsLookupAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var result = await CreateSut().Handle(
                new GetProjectWbsLookupQuery { ProjectId = 1 },
                CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NullProjectId_ReturnsAll()
        {
            var list = ProjectWorkBreakdownStructureBuilders.ValidLookupList();
            _mockIpService.Setup(x => x.GetUnitId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetWbsLookupAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var result = await CreateSut().Handle(
                new GetProjectWbsLookupQuery { ProjectId = null },
                CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            var list = new List<ProjectWbsLookupDto>();
            _mockIpService.Setup(x => x.GetUnitId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetWbsLookupAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            await CreateSut().Handle(
                new GetProjectWbsLookupQuery { ProjectId = 5 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetWbsLookupAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
