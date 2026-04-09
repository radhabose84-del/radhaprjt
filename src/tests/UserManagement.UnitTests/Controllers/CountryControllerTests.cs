using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Country.Commands.CreateCountry;
using UserManagement.Application.Country.Commands.DeleteCountry;
using UserManagement.Application.Country.Commands.UpdateCountry;
using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Application.Country.Queries.GetCountryAutoComplete;
using UserManagement.Application.Country.Queries.GetCountryById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class CountryControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private CountryController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllCountriesAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCountryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CountryDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CountryDto> { new CountryDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllCountriesAsync(1, 10);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllCountriesAsync_CallsMediatorSend_Once()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCountryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CountryDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CountryDto> { new CountryDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            await sut.GetAllCountriesAsync(1, 10);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetCountryQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCountryByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CountryDto());

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
            var command = new CreateCountryCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCountryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CountryDto());

            var sut = CreateSut();

            // Act
            var result = await sut.CreateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ValidId_ReturnsOkResult()
        {
            // Arrange
            var command = new UpdateCountryCommand { Id = 1 };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCountryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CountryDto());

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var command = new UpdateCountryCommand { Id = 0 };
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
                .Setup(m => m.Send(It.IsAny<DeleteCountryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CountryDto());

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
        public async Task GetCountry_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCountryAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CountryAutoCompleteDTO>());

            var sut = CreateSut();

            // Act
            var result = await sut.GetCountry("test");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
