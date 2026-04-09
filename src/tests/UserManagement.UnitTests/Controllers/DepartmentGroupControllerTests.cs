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

namespace UserManagement.UnitTests.Controllers
{
    public sealed class DepartmentGroupControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DepartmentController>> _mockLogger = new();

        private DepartmentGroupController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAllDepartmentGroupAsync_WithData_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllDepartmentGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAllDepartmentGroupDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetAllDepartmentGroupDto> { new GetAllDepartmentGroupDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllDepartmentGroupAsync(1, 10);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllDepartmentGroupAsync_EmptyData_ReturnsNotFound()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllDepartmentGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAllDepartmentGroupDto>>
                {
                    IsSuccess = true,
                    Message = "No records found",
                    Data = new List<GetAllDepartmentGroupDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllDepartmentGroupAsync(1, 10);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DepartmentGroupByIdDto());

            var sut = CreateSut();

            // Act
            var result = await sut.GetByIdAsync(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            // Arrange
            var command = new CreateDepartmentGroupCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDepartmentGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.CreateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            // Arrange
            var command = new CreateDepartmentGroupCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDepartmentGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            await sut.CreateAsync(command);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateDepartmentGroupCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            // Arrange
            var command = new UpdateDepartmentGroupCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateDepartmentGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DepartmentGroupByIdDto());
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDepartmentGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.Delete(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllDepartmentGroupAutocompleteAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepartmentGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DepartmentGroupAutoCompleteDto>());

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllDepartmentGroupAutocompleteAsync("test");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
