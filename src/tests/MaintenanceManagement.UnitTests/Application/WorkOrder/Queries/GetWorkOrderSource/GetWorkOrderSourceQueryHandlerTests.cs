using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderSource;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Queries.GetWorkOrderSourceTests
{
    public sealed class GetWorkOrderSourceQueryHandlerTests
    {
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetWorkOrderSourceQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetWOSourceDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetWorkOrderSourceQuery(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo.Setup(r => r.GetWOSourceDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            await CreateSut().Handle(new GetWorkOrderSourceQuery(), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetWOSourceDescAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_WithItems_ReturnsCorrectCount()
        {
            _mockQueryRepo.Setup(r => r.GetWOSourceDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster> { new() });
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto> { new() });

            var result = await CreateSut().Handle(new GetWorkOrderSourceQuery(), CancellationToken.None);

            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ReturnsSuccessMessage()
        {
            _mockQueryRepo.Setup(r => r.GetWOSourceDescAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetWorkOrderSourceQuery(), CancellationToken.None);

            result.Message.Should().Be("Success");
        }
    }
}
