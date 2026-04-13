using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Commands
{
    public sealed class CreateWorkOrderCommandHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);

        private CreateWorkOrderCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockQueryRepo.Object,
                _mockMediator.Object, _mockIp.Object, _mockUnitLookup.Object, _mockCompanyLookup.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockIp.Setup(i => i.GetCompanyId()).Returns(1);
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test-user");
            _mockIp.Setup(i => i.GetCurrentUserId()).Returns("1");
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");

            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>(It.IsAny<object>()))
                .Returns(new MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder { MiscStatus = null! });

            _mockMapper.Setup(m => m.Map<WorkOrderCombineDto>(It.IsAny<object>()))
                .Returns(new WorkOrderCombineDto());

            _mockCommandRepo.Setup(r => r.CreateAsync(
                    It.IsAny<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>(),
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder { Id = newId, MiscStatus = null! });
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsResponse()
        {
            SetupHappyPath();
            var command = new CreateWorkOrderCommand { WorkOrderDto = new WorkOrderCombineDto { RequestTypeId = 1 } };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_CreatedWithId_ReturnsSuccess()
        {
            SetupHappyPath(newId: 5);
            var command = new CreateWorkOrderCommand { WorkOrderDto = new WorkOrderCombineDto { RequestTypeId = 1 } };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CreatedWithZeroId_ReturnsFailure()
        {
            SetupHappyPath(newId: 0);
            var command = new CreateWorkOrderCommand { WorkOrderDto = new WorkOrderCombineDto { RequestTypeId = 1 } };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_CallsCreateAsyncOnce()
        {
            SetupHappyPath();
            var command = new CreateWorkOrderCommand { WorkOrderDto = new WorkOrderCombineDto { RequestTypeId = 1 } };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(
                It.IsAny<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
