using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Currency.Commands.CreateCurrency;
using UserManagement.Application.Currency.Commands.DeleteCurrency;
using UserManagement.Application.Currency.Commands.UpdateCurrency;
using UserManagement.Application.Currency.Queries.GetCurrency;
using UserManagement.Application.Currency.Queries.GetCurrencyAutoComplete;
using UserManagement.Application.Currency.Queries.GetCurrencyById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class CurrencyControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<CurrencyController>> _mockLogger = new();

        private CurrencyController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAllCurrencyAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCurrencyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CurrencyDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CurrencyDto> { new CurrencyDto { Id = 1, Code = "USD", Name = "Dollar" } },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllCurrencyAsync(1, 10);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllCurrencyAsync_EmptyData_ReturnsNotFound()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCurrencyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CurrencyDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CurrencyDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllCurrencyAsync(1, 10);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            // Arrange
            var dto = new CurrencyDto { Id = 1, Code = "USD", Name = "Dollar" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCurrencyByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

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
        public async Task GetCurrency_AutoComplete_ReturnsOkResult()
        {
            // Arrange
            var dtos = new List<CurrencyAutoCompleteDto>
            {
                new CurrencyAutoCompleteDto { Id = 1, Code = "USD" }
            };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCurrencyAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);

            var sut = CreateSut();

            // Act
            var result = await sut.GetCurrency("USD");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            // Arrange
            var command = new CreateCurrencyCommand { Code = "EUR", Name = "Euro" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCurrencyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.CreateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            // Arrange
            var command = new UpdateCurrencyCommand { Id = 1, Name = "Updated Euro", IsActive = 1 };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCurrencyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteCurrencyAsync_ReturnsOkResult()
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCurrencyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.DeleteCurrencyAsync(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
