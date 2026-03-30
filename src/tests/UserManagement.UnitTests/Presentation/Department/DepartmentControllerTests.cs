using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Presentation.Controllers;
using UserManagement.Application.Departments.Commands.CreateDepartment;
using UserManagement.Application.Departments.Commands.UpdateDepartment;
using UserManagement.Application.Departments.Commands.DeleteDepartment;
using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Application.Departments.Queries.GetDepartmentById;
using UserManagement.Application.Departments.Queries.GetDepartmentAutoCompleteSearch;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Presentation.Department
{
    public sealed class DepartmentControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DepartmentController>> _mockLogger = new(MockBehavior.Loose);

        private DepartmentController CreateSut() => new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAll_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetDepartmentDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetDepartmentDto> { DepartmentBuilders.ValidGetDepartmentDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllDepartmentAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_NoData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetDepartmentDto>>
                {
                    IsSuccess = false,
                    Message = "No Record Found",
                    Data = null,
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllDepartmentAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetDepartmentDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetDepartmentDto> { DepartmentBuilders.ValidGetDepartmentDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllDepartmentAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetDepartmentQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            var dto = DepartmentBuilders.ValidGetDepartmentDto();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentAutoCompleteSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DepartmentBuilders.ValidAutoCompleteList());

            var result = await CreateSut().GetAllDepartmentAutoCompleteSearchAsync("Test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            var command = DepartmentBuilders.ValidCreateCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDepartmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DepartmentBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ValidCommand_ReturnsOkResult()
        {
            var command = DepartmentBuilders.ValidUpdateCommand();

            // Update controller first calls GetByIdQuery, then UpdateCommand
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DepartmentBuilders.ValidGetDepartmentDto());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateDepartmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsOkResult()
        {
            // Delete controller first calls GetByIdQuery, then DeleteCommand
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DepartmentBuilders.ValidGetDepartmentDto());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDepartmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
