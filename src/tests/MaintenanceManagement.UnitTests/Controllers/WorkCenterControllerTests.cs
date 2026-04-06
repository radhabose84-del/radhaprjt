using Contracts.Common;
using MaintenanceManagement.Application.WorkCenter.Command.CreateWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.DeleteWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.UpdateWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterAutoComplete;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterById;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class WorkCenterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<CostCenterController>> _mockLogger = new(MockBehavior.Loose);

        private WorkCenterController CreateSut() => new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetWorkCenterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<WorkCenterDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllWorkcenterAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetWorkCenterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<WorkCenterDto>> { IsSuccess = true, Data = new() });

            await CreateSut().GetAllWorkcenterAsync(1, 10);
            _mockMediator.Verify(m => m.Send(It.IsAny<GetWorkCenterQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetWorkCenterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<WorkCenterDto> { IsSuccess = true });

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetWorkCenterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<WorkCenterAutoCompleteDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetWorkcenter(null);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateWorkCenterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true });

            var result = await CreateSut().CreateAsync(new CreateWorkCenterCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateWorkCenterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true });

            var result = await CreateSut().UpdateAsync(new UpdateWorkCenterCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteWorkCenterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true });

            var result = await CreateSut().DeleteWorkCenterAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
