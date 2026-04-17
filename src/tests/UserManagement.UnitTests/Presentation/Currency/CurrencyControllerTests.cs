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
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Presentation.Currency
{
    public sealed class CurrencyControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<CurrencyController>> _mockLogger = new();

        private CurrencyController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAllCurrency_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCurrencyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CurrencyDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CurrencyDto> { CurrencyBuilders.ValidDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllCurrencyAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllCurrency_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCurrencyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CurrencyDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CurrencyDto> { CurrencyBuilders.ValidDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllCurrencyAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetCurrencyQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllCurrency_EmptyData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCurrencyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CurrencyDto>>
                {
                    IsSuccess = true,
                    Message = "No records",
                    Data = new List<CurrencyDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllCurrencyAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCurrencyByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CurrencyBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetByIdAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCurrencyAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyAutoCompleteDto> { CurrencyBuilders.ValidAutoCompleteDto() });

            var result = await CreateSut().GetCurrency("USD");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            var command = CurrencyBuilders.ValidCreateCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCurrencyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            var command = CurrencyBuilders.ValidUpdateCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCurrencyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCurrencyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteCurrencyAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCurrencyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().DeleteCurrencyAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteCurrencyCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
