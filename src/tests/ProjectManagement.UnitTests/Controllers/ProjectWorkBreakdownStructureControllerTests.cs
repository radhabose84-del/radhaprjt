using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.CreateProjectWorkBreakdownStructureCommand;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.UpdateProjectWorkBreakdownStructureCommand;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetAllProjectWBS;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetById;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetByProject;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWbsLookup;
using ProjectManagement.Presentation.Controllers;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Controllers
{
    public sealed class ProjectWorkBreakdownStructureControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);
        private readonly Mock<IProjectWorkBreakdownStructureCommandRepository> _mockCommandRepo =
            new(MockBehavior.Loose);

        private ProjectWorkBreakdownStructureController CreateSut() =>
            new(_mockSender.Object, _mockCommandRepo.Object);

        // --- GetAll ---

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockSender
                .Setup(s => s.Send(It.IsAny<GetAllprojectWBSQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ProjectWorkBreakdownStructureDto>>
                {
                    IsSuccess = true,
                    Data = new List<ProjectWorkBreakdownStructureDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 20
                });

            var result = await CreateSut().GetAllAsync(1, 20, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsSenderOnce()
        {
            _mockSender
                .Setup(s => s.Send(It.IsAny<GetAllprojectWBSQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ProjectWorkBreakdownStructureDto>>
                {
                    IsSuccess = true,
                    Data = new List<ProjectWorkBreakdownStructureDto>()
                });

            await CreateSut().GetAllAsync(1, 20, null);

            _mockSender.Verify(
                s => s.Send(It.IsAny<GetAllprojectWBSQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // --- GetById ---

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkResult()
        {
            var dto = ProjectWorkBreakdownStructureBuilders.ValidDto(id: 1);
            _mockSender
                .Setup(s => s.Send(It.IsAny<GetProjectWorkBreakdownStructureByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetByProject ---

        [Fact]
        public async Task GetByProject_ReturnsOkResult()
        {
            var list = ProjectWorkBreakdownStructureBuilders.ValidDtoList(2);
            _mockSender
                .Setup(s => s.Send(It.IsAny<GetProjectWorkBreakdownStructureByProjectQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ProjectWorkBreakdownStructureDto>)list);

            var result = await CreateSut().GetByProjectAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- Autocomplete ---

        [Fact]
        public async Task GetByName_ReturnsOkResult()
        {
            var list = ProjectWorkBreakdownStructureBuilders.ValidAutocompleteList();
            _mockSender
                .Setup(s => s.Send(It.IsAny<ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Autocomplete.GetProjectWorkBreakdownStructureAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ProjectWorkBreakdownStructureAutocompleteDto>)list);

            var result = await CreateSut().GetProjectWbsByName(1, "found");

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- Create ---

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            var dto = ProjectWorkBreakdownStructureBuilders.ValidDto(id: 1);
            _mockSender
                .Setup(s => s.Send(It.IsAny<CreateProjectWorkBreakdownStructureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().CreateAsync(
                ProjectWorkBreakdownStructureBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- Update ---

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            var dto = ProjectWorkBreakdownStructureBuilders.ValidDto(id: 1);
            _mockSender
                .Setup(s => s.Send(It.IsAny<UpdateProjectWorkBreakdownStructureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().UpdateAsync(
                ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- Delete ---

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockSender
                .Setup(s => s.Send(It.IsAny<DeleteProjectWorkBreakdownStructureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsSenderOnce()
        {
            _mockSender
                .Setup(s => s.Send(It.IsAny<DeleteProjectWorkBreakdownStructureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteAsync(1);

            _mockSender.Verify(
                s => s.Send(It.IsAny<DeleteProjectWorkBreakdownStructureCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // --- WBS Lookup ---

        [Fact]
        public async Task GetLookup_ReturnsOkResult()
        {
            var list = ProjectWorkBreakdownStructureBuilders.ValidLookupList();
            _mockSender
                .Setup(s => s.Send(It.IsAny<GetProjectWbsLookupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var result = await CreateSut().GetLookup(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
