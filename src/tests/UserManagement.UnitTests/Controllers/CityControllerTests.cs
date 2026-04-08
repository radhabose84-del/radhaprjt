using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.City.Commands.CreateCity;
using UserManagement.Application.City.Commands.DeleteCity;
using UserManagement.Application.City.Commands.UpdateCity;
using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.City.Queries.GetCityAutoComplete;
using UserManagement.Application.City.Queries.GetCityById;
using UserManagement.Application.City.Queries.GetCityByStateId;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class CityControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private CityController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllCitiesAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCityQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CityDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CityDto> { new CityDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllCitiesAsync(1, 10);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllCitiesAsync_CallsMediatorSend_Once()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCityQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CityDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CityDto> { new CityDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            await sut.GetAllCitiesAsync(1, 10);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetCityQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCityByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CityDto());

            var sut = CreateSut();

            // Act
            var result = await sut.GetByIdAsync(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.GetByIdAsync(0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            // Arrange
            var command = new CreateCityCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CityDto());

            var sut = CreateSut();

            // Act
            var result = await sut.CreateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ValidStateId_ReturnsOkResult()
        {
            // Arrange
            var command = new UpdateCityCommand { StateId = 1 };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CityDto());

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_InvalidStateId_ReturnsBadRequest()
        {
            // Arrange
            var command = new UpdateCityCommand { StateId = 0 };
            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.DeleteAsync(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.DeleteAsync(0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetCity_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCityAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CityAutoCompleteDTO>());

            var sut = CreateSut();

            // Act
            var result = await sut.GetCity("test");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetStateByCountryId_ValidId_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCityByStateIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CityDto> { new CityDto() });

            var sut = CreateSut();

            // Act
            var result = await sut.GetStateByCountryId(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetStateByCountryId_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.GetStateByCountryId(0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetStateByCountryId_NullResult_ReturnsNotFound()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCityByStateIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<CityDto>?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.GetStateByCountryId(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
