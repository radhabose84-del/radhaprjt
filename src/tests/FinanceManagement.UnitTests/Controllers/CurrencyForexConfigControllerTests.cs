using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.CurrencyForexConfig.Commands.CreateCurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Commands.UpdateCurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Commands.DeleteCurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Queries.GetAllCurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Queries.GetCurrencyForexConfigById;
using FinanceManagement.Application.CurrencyForexConfig.Queries.GetCurrencyForexConfigAutoComplete;
using FinanceManagement.Application.CurrencyForexConfig.Dto;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class CurrencyForexConfigControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private CurrencyForexConfigController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllCurrencyForexConfigQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CurrencyForexConfigDto>>
                {
                    IsSuccess = true,
                    Data = new List<CurrencyForexConfigDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllCurrencyForexConfigAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCurrencyForexConfigByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CurrencyForexConfigDto());

            var result = await CreateSut().GetCurrencyForexConfigByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCurrencyForexConfigAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<CurrencyForexConfigLookupDto>)new List<CurrencyForexConfigLookupDto>());

            var result = await CreateSut().GetCurrencyForexConfigAutoCompleteAsync("for");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCurrencyForexConfigCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateCurrencyForexConfig(new CreateCurrencyForexConfigCommand
            {
                CurrencyTypeCode = "FOREX",
                CurrencyTypeName = "Forex"
            });
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCurrencyForexConfigCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateCurrencyForexConfig(new UpdateCurrencyForexConfigCommand
            {
                Id = 1,
                CurrencyTypeName = "Forex",
                IsActive = 1
            });
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCurrencyForexConfigCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteCurrencyForexConfig(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCurrencyForexConfigCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteCurrencyForexConfig(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteCurrencyForexConfigCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
