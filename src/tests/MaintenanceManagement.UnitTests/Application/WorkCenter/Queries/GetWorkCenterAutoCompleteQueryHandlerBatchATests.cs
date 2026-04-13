using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterAutoComplete;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.WorkCenter.Queries
{
    public sealed class GetWorkCenterAutoCompleteQueryHandlerBatchATests
    {
        private readonly Mock<IWorkCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetWorkCenterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetWorkCenterGroups(It.IsAny<string>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.WorkCenter>());
            _mockMapper.Setup(m => m.Map<List<WorkCenterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<WorkCenterAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new GetWorkCenterAutoCompleteQuery { SearchPattern = "test" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo.Setup(r => r.GetWorkCenterGroups("pat"))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.WorkCenter>());
            _mockMapper.Setup(m => m.Map<List<WorkCenterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<WorkCenterAutoCompleteDto>());

            await CreateSut().Handle(
                new GetWorkCenterAutoCompleteQuery { SearchPattern = "pat" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetWorkCenterGroups("pat"), Times.Once);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsMappedList()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.WorkCenter> { new() };
            var dtos = new List<WorkCenterAutoCompleteDto> { new() };

            _mockQueryRepo.Setup(r => r.GetWorkCenterGroups(It.IsAny<string>())).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<WorkCenterAutoCompleteDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(
                new GetWorkCenterAutoCompleteQuery { SearchPattern = "x" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }
    }
}
