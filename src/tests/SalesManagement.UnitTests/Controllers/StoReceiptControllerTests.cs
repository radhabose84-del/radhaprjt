using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.StoReceipt.Commands.CreateStoReceipt;
using SalesManagement.Application.StoReceipt.Dto;
using SalesManagement.Application.StoReceipt.Queries.GetAllStoReceipt;
using SalesManagement.Application.StoReceipt.Queries.GetDcOpenQty;
using SalesManagement.Application.StoReceipt.Queries.GetStoReceiptAutoComplete;
using SalesManagement.Application.StoReceipt.Queries.GetStoReceiptById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers
{
    public sealed class StoReceiptControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private StoReceiptController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllStoReceiptQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<StoReceiptHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<StoReceiptHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllStoReceiptAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStoReceiptByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((StoReceiptHeaderDto?)null);

            var result = await CreateSut().GetStoReceiptByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStoReceiptAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StoReceiptLookupDto>() as IReadOnlyList<StoReceiptLookupDto>);

            var result = await CreateSut().GetStoReceiptAutoCompleteAsync("test");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetDcOpenQty_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDcOpenQtyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DcOpenQtyDto?)null);

            var result = await CreateSut().GetDcOpenQtyAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateStoReceiptCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateStoReceipt(new CreateStoReceiptCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateStoReceiptCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            await CreateSut().CreateStoReceipt(new CreateStoReceiptCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateStoReceiptCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
