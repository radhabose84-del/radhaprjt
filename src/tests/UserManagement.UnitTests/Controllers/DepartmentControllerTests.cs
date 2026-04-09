using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Departments.Commands.CreateDepartment;
using UserManagement.Application.Departments.Commands.DeleteDepartment;
using UserManagement.Application.Departments.Commands.UpdateDepartment;
using UserManagement.Application.Departments.Queries.GetDepartmentAutoCompleteSearch;
using UserManagement.Application.Departments.Queries.GetDepartmentByDepartmentGroupId;
using UserManagement.Application.Departments.Queries.GetDepartmentByGroupWithControl;
using UserManagement.Application.Departments.Queries.GetDepartmentById;
using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class DepartmentControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DepartmentController>> _mockLogger = new();

        private DepartmentController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAllDepartmentAsync_WithData_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetDepartmentDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetDepartmentDto> { new GetDepartmentDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllDepartmentAsync(1, 10);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllDepartmentAsync_EmptyData_ReturnsNotFound()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetDepartmentDto>>
                {
                    IsSuccess = true,
                    Message = "No records found",
                    Data = new List<GetDepartmentDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllDepartmentAsync(1, 10);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetDepartmentDto());

            var sut = CreateSut();

            // Act
            var result = await sut.GetByIdAsync(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllDepartmentAutoCompleteSearchAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentAutoCompleteSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DepartmentAutocompleteDto>());

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllDepartmentAutoCompleteSearchAsync("test");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllDepartment_WithData_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentwithoutDataControl>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DepartmentAutocompleteDto>>
                {
                    IsSuccess = true,
                    Data = new List<DepartmentAutocompleteDto> { new DepartmentAutocompleteDto() }
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllDepartment("test");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllDepartment_NotSuccess_ReturnsNotFound()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentwithoutDataControl>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DepartmentAutocompleteDto>>
                {
                    IsSuccess = false,
                    Data = new List<DepartmentAutocompleteDto>()
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllDepartment("test");

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetDepartmentsByDepartmentGroupId_ValidName_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentsByDepartmentGroupIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DepartmentWithGroupDto>>
                {
                    IsSuccess = true,
                    Data = new List<DepartmentWithGroupDto> { new DepartmentWithGroupDto() }
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetDepartmentsByDepartmentGroupId("Maintenance");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetDepartmentsByDepartmentGroupId_EmptyName_ReturnsBadRequest()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.GetDepartmentsByDepartmentGroupId("");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            // Arrange
            var command = new CreateDepartmentCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDepartmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DepartmentDto());

            var sut = CreateSut();

            // Act
            var result = await sut.CreateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ExistingDepartment_ReturnsOkResult()
        {
            // Arrange
            var command = new UpdateDepartmentCommand { Id = 1 };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetDepartmentDto());
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateDepartmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_NonExistingDepartment_ReturnsNotFound()
        {
            // Arrange
            var command = new UpdateDepartmentCommand { Id = 999 };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetDepartmentDto?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(command);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_ExistingDepartment_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetDepartmentDto());
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDepartmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.Delete(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_NonExistingDepartment_ReturnsNotFound()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetDepartmentDto?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.Delete(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetDepartmentsByDepartmentGroupWithControl_ValidName_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentByGroupWithControlQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DepartmentWithControlByGroupDto>>
                {
                    IsSuccess = true,
                    Data = new List<DepartmentWithControlByGroupDto>()
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetDepartmentsByDepartmentGroupWithControl("Maintenance");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetDepartmentsByDepartmentGroupWithControl_EmptyName_ReturnsBadRequest()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.GetDepartmentsByDepartmentGroupWithControl("");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
