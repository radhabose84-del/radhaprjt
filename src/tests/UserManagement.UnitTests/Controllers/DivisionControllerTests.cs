using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Divisions.Commands.CreateDivision;
using UserManagement.Application.Divisions.Commands.DeleteDivision;
using UserManagement.Application.Divisions.Commands.UpdateDivision;
using UserManagement.Application.Divisions.Queries.GetDivisionAutoComplete;
using UserManagement.Application.Divisions.Queries.GetDivisionById;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using UserManagement.Application.Divisions.Queries.GetUnitsByDivision;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class DivisionControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private DivisionController CreateSut()
        {
            var controller = new DivisionController(_mockMediator.Object);
            var claims = new List<Claim>
            {
                new Claim("companyId", "1"),
                new Claim("CompanyId", "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
            return controller;
        }

        [Fact]
        public async Task GetAllDivisionsAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDivisionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DivisionDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<DivisionDTO> { new DivisionDTO() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllDivisionsAsync(1, 10);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllDivisionsAsync_CallsMediatorSend_Once()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDivisionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DivisionDTO>>
                {
                    IsSuccess = true,
                    Data = new List<DivisionDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            await sut.GetAllDivisionsAsync(1, 10);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetDivisionQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDivisionByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DivisionDTO());

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
            var command = new CreateDivisionCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDivisionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DivisionDTO());

            var sut = CreateSut();

            // Act
            var result = await sut.CreateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ExistingDivision_ReturnsOkResult()
        {
            // Arrange
            var command = new UpdateDivisionCommand { Id = 1 };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDivisionByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DivisionDTO());
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateDivisionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.Update(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_NonExistingDivision_ReturnsNotFound()
        {
            // Arrange
            var command = new UpdateDivisionCommand { Id = 999 };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDivisionByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DivisionDTO?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.Update(command);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDivisionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.Delete(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDivisionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            await sut.Delete(1);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteDivisionCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetDivision_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDivisionAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DivisionAutoCompleteDTO>());

            var sut = CreateSut();

            // Act
            var result = await sut.GetDivision("test");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUnitsByDivisionAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUnitsByDivisionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetUnitsByDivisionDto>());

            var sut = CreateSut();

            // Act
            var result = await sut.GetUnitsByDivisionAsync(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
