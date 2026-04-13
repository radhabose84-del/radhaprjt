using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MaintenanceManagement.Application.WorkOrder.Queries.GetRequestType;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Queries.GetRequestTypeTests
{
    public sealed class GetRequestTypeQueryHandlerTests
    {
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetRequestTypeQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.MiscMaster> { new(), new() };
            _mockQueryRepo.Setup(r => r.GetRequestTypeAsync()).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto> { new(), new() });

            var result = await CreateSut().Handle(new GetRequestTypeQuery(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo.Setup(r => r.GetRequestTypeAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            await CreateSut().Handle(new GetRequestTypeQuery(), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetRequestTypeAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsSuccessWithZeroCount()
        {
            _mockQueryRepo.Setup(r => r.GetRequestTypeAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetRequestTypeQuery(), CancellationToken.None);

            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ReturnsMessageSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetRequestTypeAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetRequestTypeQuery(), CancellationToken.None);

            result.Message.Should().Be("Success");
        }
    }
}
