using Contracts.Commands.Finance;
using Contracts.Common;
using Contracts.Dtos.Finance;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.Invoice.Commands.CreateInvoice;
using SalesManagement.Application.Invoice.Commands.UpdateInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Application.Invoice.Queries.GetAllInvoice;
using SalesManagement.Application.Invoice.Queries.GetInvoiceAutoComplete;
using SalesManagement.Application.Invoice.Queries.GetInvoiceById;
using SalesManagement.Application.Invoice.Queries.GetInvoiceGatePassPending;
using SalesManagement.Application.Invoice.Queries.GetInvoicePending;
using SalesManagement.Application.Invoice.Queries.GetInvoicePrintDetails;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers
{
    public sealed class InvoiceControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private InvoiceController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllInvoiceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<InvoiceHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<InvoiceHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllInvoiceAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetInvoiceByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InvoiceHeaderDto());

            var result = await CreateSut().GetInvoiceByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetInvoiceAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InvoiceLookupDto>() as IReadOnlyList<InvoiceLookupDto>);

            var result = await CreateSut().GetInvoiceAutoCompleteAsync("test");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateInvoiceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateInvoice(new CreateInvoiceCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateInvoiceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateInvoice(new UpdateInvoiceCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetInvoicePendingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<GetInvoicePendingDto>(), 0));

            var result = await CreateSut().GetPendingAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPrintDetails_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetInvoicePrintDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InvoicePrintDto?)null);

            var result = await CreateSut().GetInvoicePrintDetailsAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetGatePassPending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetInvoiceGatePassPendingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetInvoiceGatePassPendingDto>());

            var result = await CreateSut().GetGatePassPendingAsync("KA01");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GenerateEInvoice_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEInvoiceFromSalesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<EInvoiceCreationResultDto> { IsSuccess = true });

            var result = await CreateSut().GenerateEInvoice(1, false);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateInvoiceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            await CreateSut().CreateInvoice(new CreateInvoiceCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateInvoiceCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
