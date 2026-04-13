using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.MachineGroup.Queries.GetActivityMasterAutoComplete;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupAutoComplete;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ActivityMaster.Queries
{
    public sealed class GetActivityMasterAutoCompleteQueryHandlerBatchATests
    {
        private readonly Mock<IActivityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetActivityMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetActivityMasterAutoComplete(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.ActivityMaster>());
            _mockMapper.Setup(m => m.Map<List<GetActivityMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<GetActivityMasterAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new GetActivityMasterAutoCompleteQuery { SearchPattern = "test", MachineCode = "M1" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryWithSearchPattern()
        {
            _mockQueryRepo.Setup(r => r.GetActivityMasterAutoComplete("pat", "MC"))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.ActivityMaster>());
            _mockMapper.Setup(m => m.Map<List<GetActivityMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<GetActivityMasterAutoCompleteDto>());

            await CreateSut().Handle(
                new GetActivityMasterAutoCompleteQuery { SearchPattern = "pat", MachineCode = "MC" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetActivityMasterAutoComplete("pat", "MC"), Times.Once);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsMappedList()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.ActivityMaster>
            {
                new() { Id = 1 }
            };
            var dtos = new List<GetActivityMasterAutoCompleteDto> { new() };

            _mockQueryRepo.Setup(r => r.GetActivityMasterAutoComplete(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetActivityMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetActivityMasterAutoCompleteQuery { SearchPattern = "any", MachineCode = "any" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
