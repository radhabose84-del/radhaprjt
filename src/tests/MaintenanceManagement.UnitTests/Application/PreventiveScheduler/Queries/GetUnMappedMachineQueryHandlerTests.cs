using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetUnMappedMachine;
using MaintenanceManagement.Domain.Entities;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.PreventiveScheduler.Queries
{
    public sealed class GetUnMappedMachineQueryHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetUnMappedMachineQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.MachineMaster> { new() };
            var dtos = new List<UnMappedMachineDto> { new() };
            _mockQueryRepo.Setup(r => r.GetUnMappedMachineIdByCode(1)).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<UnMappedMachineDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetUnMappedMachineQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetUnMappedMachineIdByCode(1)).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MachineMaster>());
            _mockMapper.Setup(m => m.Map<List<UnMappedMachineDto>>(It.IsAny<object>())).Returns(new List<UnMappedMachineDto>());

            var result = await CreateSut().Handle(new GetUnMappedMachineQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
