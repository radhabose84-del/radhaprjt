using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMasterAutoComplete;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ShiftMaster.Queries
{
    public sealed class GetShiftMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IShiftMasterQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetShiftMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.ShiftMaster> { new() { ShiftCode = "S1", ShiftName = "Morning" } };
            var dtos = new List<ShiftMasterAutoCompleteDTO> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetShiftMaster(It.IsAny<string>())).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<ShiftMasterAutoCompleteDTO>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetShiftMasterAutoCompleteQuery { SearchPattern = "Morn" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetShiftMaster(It.IsAny<string>())).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.ShiftMaster>());
            _mockMapper.Setup(m => m.Map<List<ShiftMasterAutoCompleteDTO>>(It.IsAny<object>())).Returns(new List<ShiftMasterAutoCompleteDTO>());

            var result = await CreateSut().Handle(new GetShiftMasterAutoCompleteQuery { SearchPattern = "NONE" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
