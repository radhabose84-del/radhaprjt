using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetWorkflowTypeAutoComplete;
using Contracts.Interfaces;

namespace BackgroundService.UnitTests.Application.Workflow.WorkflowType.Queries
{
    public sealed class GetWorkflowTypeAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IWorkflowTypeQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILookupRepository> _mockLookupRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpAddressService = new(MockBehavior.Loose);

        private GetWorkflowTypeAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockLookupRepo.Object, _mockIpAddressService.Object);

        [Fact]
        public async Task Handle_ReturnsMappedList()
        {
            var entities = new List<BackgroundService.Domain.Entities.Workflow.WorkflowType>
            {
                new() { Id = 1 }
            };

            _mockQueryRepo
                .Setup(r => r.GetWorkflowTypeAutoComplete(It.IsAny<string>()))
                .ReturnsAsync(entities);

            var dtos = new List<GetWorkflowTypeAutoCompleteDto>
            {
                new() { Id = 1, MenuId = 100, MenuName = "Test" }
            };

            _mockMapper
                .Setup(m => m.Map<List<GetWorkflowTypeAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockLookupRepo
                .Setup(r => r.GetMenuNamesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string> { { 100, "Test Menu" } });

            var sut = CreateSut();

            var result = await sut.Handle(
                new GetWorkflowTypeAutoCompleteQuery { SearchPattern = "test" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptySearch_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetWorkflowTypeAutoComplete(string.Empty))
                .ReturnsAsync(new List<BackgroundService.Domain.Entities.Workflow.WorkflowType>());

            _mockMapper
                .Setup(m => m.Map<List<GetWorkflowTypeAutoCompleteDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Workflow.WorkflowType>>()))
                .Returns(new List<GetWorkflowTypeAutoCompleteDto>());

            _mockLookupRepo
                .Setup(r => r.GetMenuNamesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, string>());

            var sut = CreateSut();

            var result = await sut.Handle(
                new GetWorkflowTypeAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
