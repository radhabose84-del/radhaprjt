using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderStatus;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Queries.GetWorkOrderStatusTests
{
    public sealed class GetWorkOrderStatusQueryHandlerTests
    {
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetWorkOrderStatusQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetWOStatusDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetWorkOrderStatusQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo.Setup(r => r.GetWOStatusDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            await CreateSut().Handle(new GetWorkOrderStatusQuery(), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetWOStatusDescAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_WithItems_ReturnsCorrectCount()
        {
            _mockQueryRepo.Setup(r => r.GetWOStatusDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster> { new(), new() });
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto> { new(), new() });

            var result = await CreateSut().Handle(new GetWorkOrderStatusQuery(), CancellationToken.None);

            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_EmptyList_DataIsEmpty()
        {
            _mockQueryRepo.Setup(r => r.GetWOStatusDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetWorkOrderStatusQuery(), CancellationToken.None);

            result.Data.Should().BeEmpty();
        }
    }
}
