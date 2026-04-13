using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOderDropdown;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Queries.GetWorkOderDropdownTests
{
    public sealed class GetWorkOderDropdownQueryHandlerTests
    {
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetWorkOderDropdownQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetWorkOrderAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>());
            _mockMapper.Setup(m => m.Map<List<GetWorkOderDropdownDto>>(It.IsAny<object>()))
                .Returns(new List<GetWorkOderDropdownDto>());

            var result = await CreateSut().Handle(new GetWorkOderDropdownQuery(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo.Setup(r => r.GetWorkOrderAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>());
            _mockMapper.Setup(m => m.Map<List<GetWorkOderDropdownDto>>(It.IsAny<object>()))
                .Returns(new List<GetWorkOderDropdownDto>());

            await CreateSut().Handle(new GetWorkOderDropdownQuery(), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetWorkOrderAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_WithItems_ReturnsCorrectCount()
        {
            _mockQueryRepo.Setup(r => r.GetWorkOrderAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>
                {
                    new() { MiscStatus = null! },
                    new() { MiscStatus = null! },
                    new() { MiscStatus = null! }
                });
            _mockMapper.Setup(m => m.Map<List<GetWorkOderDropdownDto>>(It.IsAny<object>()))
                .Returns(new List<GetWorkOderDropdownDto> { new(), new(), new() });

            var result = await CreateSut().Handle(new GetWorkOderDropdownQuery(), CancellationToken.None);

            result.TotalCount.Should().Be(3);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetWorkOrderAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>());
            _mockMapper.Setup(m => m.Map<List<GetWorkOderDropdownDto>>(It.IsAny<object>()))
                .Returns(new List<GetWorkOderDropdownDto>());

            var result = await CreateSut().Handle(new GetWorkOderDropdownQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.TotalCount.Should().Be(0);
        }
    }
}
