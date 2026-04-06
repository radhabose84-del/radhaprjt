using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using Contracts.Commands.Finance;
using Contracts.Dtos.Finance;
using FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.UpdateEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.DeleteEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.GenerateIrn;
using FinanceManagement.Application.EInvoiceHeader.Commands.GenerateEwb;
using FinanceManagement.Application.EInvoiceHeader.Commands.CancelIrn;
using FinanceManagement.Application.EInvoiceHeader.Commands.CancelEwb;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetAllEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetEInvoiceHeaderById;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetEInvoiceHeaderAutoComplete;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetIrnDetails;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetEwbDetails;
using FinanceManagement.Application.EInvoiceHeader.Dto;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class EInvoiceHeaderControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private EInvoiceHeaderController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllEInvoiceHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<EInvoiceHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<EInvoiceHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllEInvoiceHeaderAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllEInvoiceHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<EInvoiceHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<EInvoiceHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllEInvoiceHeaderAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllEInvoiceHeaderQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEInvoiceHeaderByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EInvoiceHeaderDto());

            var result = await CreateSut().GetEInvoiceHeaderByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEInvoiceHeaderAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<EInvoiceHeaderLookupDto>)new List<EInvoiceHeaderLookupDto>());

            var result = await CreateSut().GetEInvoiceHeaderAutoCompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEInvoiceHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateEInvoiceHeader(new CreateEInvoiceHeaderCommand
            {
                UnitId = 1,
                PartyId = 1,
                InvoiceNo = "INV001"
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateEInvoiceHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateEInvoiceHeader(new UpdateEInvoiceHeaderCommand
            {
                Id = 1,
                UnitId = 1,
                PartyId = 1,
                InvoiceNo = "INV001",
                IsActive = 1
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteEInvoiceHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteEInvoiceHeader(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteEInvoiceHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteEInvoiceHeader(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteEInvoiceHeaderCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateIrn_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GenerateIrnCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<NicIrnResultDto> { IsSuccess = true, Message = "IRN generated" });

            var result = await CreateSut().GenerateIrn(new GenerateIrnCommand { EInvoiceHeaderId = 1 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GenerateEwb_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GenerateEwbCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<NicEwbResultDto> { IsSuccess = true, Message = "EWB generated" });

            var result = await CreateSut().GenerateEwb(new GenerateEwbCommand { EInvoiceHeaderId = 1, Distance = 10 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CancelIrn_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CancelIrnCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<NicCancelIrnResultDto> { IsSuccess = true, Message = "IRN cancelled" });

            var result = await CreateSut().CancelIrn(new CancelIrnCommand { EInvoiceHeaderId = 1, CnlRsn = "1" });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CancelEwb_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CancelEwbCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<NicCancelEwbResultDto> { IsSuccess = true, Message = "EWB cancelled" });

            var result = await CreateSut().CancelEwb(new CancelEwbCommand { EInvoiceHeaderId = 1, CancelRsnCode = 1 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetIrnDetails_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetIrnDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<object> { IsSuccess = true, Message = "Success" });

            var result = await CreateSut().GetIrnDetails(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetEwbDetails_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEwbDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<object> { IsSuccess = true, Message = "Success" });

            var result = await CreateSut().GetEwbDetails(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateEInvoiceFromSales_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEInvoiceFromSalesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<EInvoiceCreationResultDto> { IsSuccess = true, Message = "Created from sales" });

            var result = await CreateSut().CreateEInvoiceFromSales(new CreateEInvoiceFromSalesCommand
            {
                InvoiceId = 1,
                UnitId = 1,
                SalesOrderType = 1
            });

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
