using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.DepartmentGroup.Command.CreateDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Command.DeleteDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Command.UpdateDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Queries.GetAllDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Queries.GetDepartmentGroupAutoSearch;
using UserManagement.Application.DepartmentGroup.Queries.GetDepartmentGroupById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.DepartmentGroup
{
    public sealed class DepartmentGroupControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DepartmentController>> _mockLogger = new();

        private DepartmentGroupController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDepartmentGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateDepartmentGroupCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllDepartmentGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAllDepartmentGroupDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetAllDepartmentGroupDto> { new() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllDepartmentGroupAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_EmptyData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllDepartmentGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAllDepartmentGroupDto>>
                {
                    IsSuccess = false,
                    Data = new List<GetAllDepartmentGroupDto>(),
                    Message = "Not found."
                });

            var result = await CreateSut().GetAllDepartmentGroupAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DepartmentGroupByIdDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateDepartmentGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(new UpdateDepartmentGroupCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DepartmentGroupByIdDto());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDepartmentGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DepartmentGroupAutoCompleteDto>());

            var result = await CreateSut().GetAllDepartmentGroupAutocompleteAsync("Test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
