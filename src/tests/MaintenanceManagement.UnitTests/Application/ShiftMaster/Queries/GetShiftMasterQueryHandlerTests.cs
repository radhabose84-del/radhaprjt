using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ShiftMaster.Queries
{
    public sealed class GetShiftMasterQueryHandlerTests
    {
        private readonly Mock<IShiftMasterQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetShiftMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.ShiftMaster> { new() { ShiftCode = "S1", ShiftName = "Morning" } };
            var dtos = new List<ShiftMasterDTO> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllShiftMasterAsync(1, 10, null)).ReturnsAsync((entities, 1));
            _mockMapper.Setup(m => m.Map<List<ShiftMasterDTO>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetShiftMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsPaginationMetadata()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.ShiftMaster> { new() { ShiftCode = "S1", ShiftName = "Morning" } };
            var dtos = new List<ShiftMasterDTO> { new() };
            _mockQueryRepo.Setup(r => r.GetAllShiftMasterAsync(2, 5, "morn")).ReturnsAsync((entities, 11));
            _mockMapper.Setup(m => m.Map<List<ShiftMasterDTO>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetShiftMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "morn" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetAllShiftMasterAsync(1, 10, null)).ReturnsAsync((new List<MaintenanceManagement.Domain.Entities.ShiftMaster>(), 0));
            _mockMapper.Setup(m => m.Map<List<ShiftMasterDTO>>(It.IsAny<object>())).Returns(new List<ShiftMasterDTO>());

            var result = await CreateSut().Handle(new GetShiftMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
