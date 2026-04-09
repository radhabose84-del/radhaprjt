using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.DispatchAdvice.Commands.CreateDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Commands.DeleteDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Application.DispatchAdvice.Queries.GetAllDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceAutoComplete;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceById;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackNoValidation;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackRange;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceStock;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers
{
    public sealed class DispatchAdviceControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DispatchAdviceController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllDispatchAdviceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DispatchAdviceHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<DispatchAdviceHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllDispatchAdviceAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDispatchAdviceByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DispatchAdviceHeaderDto?)null);

            var result = await CreateSut().GetDispatchAdviceByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDispatchAdviceAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DispatchAdviceLookupDto>() as IReadOnlyList<DispatchAdviceLookupDto>);

            var result = await CreateSut().GetDispatchAdviceAutoCompleteAsync("test");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetStock_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDispatchAdviceStockQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DispatchAdviceStockDto>());

            var result = await CreateSut().GetDispatchAdviceStockAsync(1, 1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ValidatePackNo_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDispatchAdvicePackNoValidationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PackNoValidationDto { IsValid = true });

            var result = await CreateSut().ValidatePackNoAsync(1, 1, 1, 10, 1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPackRange_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDispatchAdvicePackRangeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DispatchAdvicePackRangeDto>());

            var result = await CreateSut().GetDispatchAdvicePackRangeAsync(1, 1, 1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDispatchAdviceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateDispatchAdvice(new CreateDispatchAdviceCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDispatchAdviceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteDispatchAdvice(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDispatchAdviceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            await CreateSut().CreateDispatchAdvice(new CreateDispatchAdviceCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateDispatchAdviceCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
