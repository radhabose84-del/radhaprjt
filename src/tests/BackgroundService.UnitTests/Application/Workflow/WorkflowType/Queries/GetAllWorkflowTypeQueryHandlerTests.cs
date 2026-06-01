using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetAllWorkflowType;

namespace BackgroundService.UnitTests.Application.Workflow.WorkflowType.Queries
{
    public sealed class GetAllWorkflowTypeQueryHandlerTests
    {
        private readonly Mock<IWorkflowTypeQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILookupRepository> _mockLookupRepo = new(MockBehavior.Loose);
        private readonly Mock<Contracts.Interfaces.IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetAllWorkflowTypeQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockLookupRepo.Object, _mockIpService.Object);

        private void SetupHappyPath(int count = 1)
        {
            var entities = new List<BackgroundService.Domain.Entities.Workflow.WorkflowType>();
            for (int i = 0; i < count; i++)
                entities.Add(new BackgroundService.Domain.Entities.Workflow.WorkflowType { Id = i + 1 });

            _mockQueryRepo
                .Setup(r => r.GetAllWorkflowTypeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((entities, count));

            var dtos = new List<WorkflowTypeDto>();
            for (int i = 0; i < count; i++)
                dtos.Add(new WorkflowTypeDto { Id = i + 1, MenuId = 100 });

            _mockMapper
                .Setup(m => m.Map<List<WorkflowTypeDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Workflow.WorkflowType>>()))
                .Returns(dtos);

            _mockIpService.Setup(s => s.GetUserId()).Returns(1);

            // MenuId on entities defaults to 0 — include 0 so they pass the access filter
            _mockLookupRepo
                .Setup(r => r.GetUserAccessibleMenuIdsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HashSet<int> { 0, 100 });

            _mockLookupRepo
                .Setup(r => r.GetMenuNamesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string> { { 100, "Test Menu" } });
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            var result = await sut.Handle(
                new GetAllWorkflowTypeQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            SetupHappyPath(0);
            var sut = CreateSut();

            var result = await sut.Handle(
                new GetAllWorkflowTypeQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            var result = await sut.Handle(
                new GetAllWorkflowTypeQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }
    }
}
