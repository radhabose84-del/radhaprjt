using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupAutoComplete;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineGroup.Queries
{
    public sealed class GetMachineGroupByIdQueryHandlerTests
    {
        private readonly Mock<IMachineGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMachineGroupByIdQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new MaintenanceManagement.Domain.Entities.MachineGroup { Id = 1 });
            _mockMapper.Setup(m => m.Map<GetMachineGroupByIdDto>(It.IsAny<object>())).Returns(new GetMachineGroupByIdDto());

            var result = await CreateSut().Handle(new GetMachineGroupByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
        }
    }

    public sealed class GetMachineGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMachineGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMachineGroupAutoCompleteQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            _mockQueryRepo.Setup(r => r.GetMachineGroupAutoComplete(It.IsAny<string>())).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MachineGroup> { new() });
            _mockMapper.Setup(m => m.Map<List<GetMachineGroupAutoCompleteDto>>(It.IsAny<object>())).Returns(new List<GetMachineGroupAutoCompleteDto> { new() });

            var result = await CreateSut().Handle(new GetMachineGroupAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
