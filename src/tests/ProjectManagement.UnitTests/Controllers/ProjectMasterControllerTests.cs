using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.ProjectMaster.Command.DeleteProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMasterById;
using ProjectManagement.Presentation.Controllers;

namespace ProjectManagement.UnitTests.Controllers
{
    public sealed class ProjectMasterControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private ProjectMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetProjectMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetProjectMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetProjectMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 20
                });

            var result = await CreateSut().GetAllProjectMasterAsync(1, 20);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetProjectMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProjectMasterDto { Id = 1, ProjectCode = "PRJ001", ProjectName = "Test" });

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_Success_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteProjectMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteProjectMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().DeleteAsync(999);
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
