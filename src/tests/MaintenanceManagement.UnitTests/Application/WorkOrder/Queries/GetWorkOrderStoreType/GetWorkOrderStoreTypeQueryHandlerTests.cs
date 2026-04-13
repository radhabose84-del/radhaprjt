using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderStoreType;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Queries.GetWorkOrderStoreTypeTests
{
    public sealed class GetWorkOrderStoreTypeQueryHandlerTests
    {
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetWorkOrderStoreTypeQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetWOStoreTypeDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetWorkOrderStoreTypeQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo.Setup(r => r.GetWOStoreTypeDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            await CreateSut().Handle(new GetWorkOrderStoreTypeQuery(), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetWOStoreTypeDescAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_WithItems_ReturnsCorrectCount()
        {
            _mockQueryRepo.Setup(r => r.GetWOStoreTypeDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster> { new(), new(), new() });
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto> { new(), new(), new() });

            var result = await CreateSut().Handle(new GetWorkOrderStoreTypeQuery(), CancellationToken.None);

            result.TotalCount.Should().Be(3);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsZeroCount()
        {
            _mockQueryRepo.Setup(r => r.GetWOStoreTypeDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetWorkOrderStoreTypeQuery(), CancellationToken.None);

            result.TotalCount.Should().Be(0);
        }
    }
}
