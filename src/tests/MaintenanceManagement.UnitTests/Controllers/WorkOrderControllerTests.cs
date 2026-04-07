using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.DeleteFileWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.DeleteFileWorkOrder.Item;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
// UpdateWorkOrderRequestDate not tested - complex multi-step date update
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder.Item;
using MaintenanceManagement.Application.WorkOrder.Queries.GetRequestType;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOderDropdown;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderById;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderRootCause;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderSource;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderStatus;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderStoreType;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class WorkOrderControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IWorkOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        private WorkOrderController CreateSut() => new(_mockMediator.Object, _mockCommandRepo.Object);

        [Fact]
        public async Task GetWorkOrderStatus_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetWorkOrderStatusQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>> { IsSuccess = true, Data = new() { new() } });

            var result = await CreateSut().GetWorkOrderStatus();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetWorkOrderRootCause_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetWorkOrderRootCauseQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>> { IsSuccess = true, Data = new() { new() } });

            var result = await CreateSut().GetWorkOrderRootCause();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetWorkOrderSource_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetWorkOrderSourceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>> { IsSuccess = true, Data = new() { new() } });

            var result = await CreateSut().GetWorkOrderSource();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetStoreType_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetWorkOrderStoreTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>> { IsSuccess = true, Data = new() { new() } });

            var result = await CreateSut().GetStoreType();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetRequestType_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRequestTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>> { IsSuccess = true, Data = new() { new() } });

            var result = await CreateSut().GetRequestType();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetWorkOrderByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetWorkOrderByIdDto> { IsSuccess = true });

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetWorkOrderDropdown_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetWorkOderDropdownQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetWorkOderDropdownDto>> { IsSuccess = true, Data = new() { new() } });

            var result = await CreateSut().GetWorkOrderAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateWorkOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<WorkOrderCombineDto> { IsSuccess = true });

            var result = await CreateSut().CreateAsync(new CreateWorkOrderCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateWorkOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true });

            var result = await CreateSut().UpdateAsync(new UpdateWorkOrderCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteLogo_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteFileWorkOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true });

            var result = await CreateSut().DeleteLogo(new DeleteFileWorkOrderCommand { Image = "test.jpg" });
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteItemLogo_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteFileItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true });

            var result = await CreateSut().DeleteItemLogo(new DeleteFileItemCommand { Image = "test.jpg" });
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
