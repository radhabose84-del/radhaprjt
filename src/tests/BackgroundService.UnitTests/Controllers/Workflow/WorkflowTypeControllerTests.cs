using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BackgroundService.Presentation.Controllers.Workflow;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.CreateWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.DeleteWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.UpdateWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetAllWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetWorkflowTypeAutoComplete;

namespace BackgroundService.UnitTests.Controllers.Workflow
{
    public sealed class WorkflowTypeControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private WorkflowTypeController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllWorkflowTypeAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllWorkflowTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<WorkflowTypeDto>>
                {
                    IsSuccess = true,
                    Data = new List<WorkflowTypeDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllWorkflowTypeAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllWorkflowTypeAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllWorkflowTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<WorkflowTypeDto>>
                {
                    IsSuccess = true,
                    Data = new List<WorkflowTypeDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllWorkflowTypeAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllWorkflowTypeQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetWorkflowTypeAutoCompleteAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetWorkflowTypeAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetWorkflowTypeAutoCompleteDto>());

            var result = await CreateSut().GetWorkflowTypeAutoCompleteAsync("TestModule");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateWorkflowTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int> { 1 });

            var result = await CreateSut().CreateAsync(new CreateWorkflowTypeCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateWorkflowTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(new UpdateWorkflowTypeCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteWorkflowTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteWorkflowTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteWorkflowTypeCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
