using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.DeleteDocument;
using ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster;
using ProjectManagement.Application.ProjectMaster.Command.DeleteProjectMaster;
using ProjectManagement.Application.ProjectMaster.Command.UpdateProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMasterById;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectPendingApprovals;
using ProjectManagement.Application.ProjectMaster.Queries.ProjectMasterAutoComplete;
using ProjectManagement.Application.UploadDocument;
using ProjectManagement.Presentation.Controllers;
using ProjectManagement.UnitTests.TestData;

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

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateProjectMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProjectMasterDto { Id = 1, ProjectCode = "PRJ001" });

            var result = await CreateSut().CreateAsync(ProjectMasterBuilders.ValidCreateCommand().Project);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateProjectMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProjectMasterDto { Id = 1, ProjectCode = "PRJ001" });

            await CreateSut().CreateAsync(ProjectMasterBuilders.ValidCreateCommand().Project);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateProjectMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateProjectMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProjectMasterDto { Id = 1, ProjectCode = "PRJ001" });

            var result = await CreateSut().UpdateAsync(ProjectMasterBuilders.ValidUpdateCommand().Project!);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetProjectMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProjectMasterAutoCompleteDto> { ProjectMasterBuilders.ValidAutoCompleteDto() });

            var result = await CreateSut().GetAutoCompleteAsync(null, null, "Test", null, null, CancellationToken.None);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UploadDocument_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UploadDocumentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DocumentDto { FileName = "test.pdf", PODocumentBase64 = "base64string" });

            var command = new UploadDocumentCommand { File = new Mock<IFormFile>().Object };
            var result = await CreateSut().UploadDocument(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteDocument_NullCommand_ReturnsBadRequest()
        {
            var result = await CreateSut().DeleteDocument(null!);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteDocument_EmptyPath_ReturnsBadRequest()
        {
            var command = new DeleteDocumentCommand { ProjectDocumentPath = "" };

            var result = await CreateSut().DeleteDocument(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteDocument_ValidCommand_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDocumentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new DeleteDocumentCommand
            {
                Id = 1,
                ProjectId = 1,
                ProjectDocumentPath = "project/doc.pdf",
                FileName = "doc.pdf"
            };
            var result = await CreateSut().DeleteDocument(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingApprovals_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetProjectPendingApprovalQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<GetProjectPendingApprovalDto>(), 0));

            var result = await CreateSut().GetPendingApprovals(new GetProjectPendingApprovalQuery(), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
