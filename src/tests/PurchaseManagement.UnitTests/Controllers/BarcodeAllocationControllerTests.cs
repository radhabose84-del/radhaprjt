using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.BarcodeAllocation.Command.CreateBarcodeAllocation;
using PurchaseManagement.Application.BarcodeAllocation.Command.DeleteBarcodeAllocation;
using PurchaseManagement.Application.BarcodeAllocation.Command.UpdateBarcodeAllocation;
using PurchaseManagement.Application.BarcodeAllocation.Dto;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetAllocationBarcodeSeries;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetAllocationEmployees;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocation;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationAutoComplete;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationById;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationLabels;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetNextAllocationFrom;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetNextAllocationNumber;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class BarcodeAllocationControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private BarcodeAllocationController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBarcodeAllocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<BarcodeAllocationDto>> { IsSuccess = true, Data = new List<BarcodeAllocationDto>() });

            (await CreateSut().GetAllBarcodeAllocationAsync(1, 10)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBarcodeAllocationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BarcodeAllocationBuilders.ValidDto());

            (await CreateSut().GetByIdAsync(1)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetLabels_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBarcodeAllocationLabelsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BarcodeLabelReportDto());

            (await CreateSut().GetLabelsAsync(7)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBarcodeAllocationAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BarcodeAllocationBuilders.ValidLookupList());

            (await CreateSut().GetAutoCompleteAsync("BBA")).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Employees_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllocationEmployeesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<BarcodeAllocationEmployeeDto>)new List<BarcodeAllocationEmployeeDto>());

            (await CreateSut().GetEmployeesAsync("Raj")).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task BarcodeSeries_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllocationBarcodeSeriesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<BarcodeAllocationSeriesDto>)new List<BarcodeAllocationSeriesDto>());

            (await CreateSut().GetBarcodeSeriesAsync(null)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task NextNumber_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNextAllocationNumberQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("BBA-2025-0008");

            (await CreateSut().GetNextNumberAsync(null)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task NextFrom_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNextAllocationFromQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(25000501L);

            (await CreateSut().GetNextFromAsync(1)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateBarcodeAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            (await CreateSut().CreateAsync(BarcodeAllocationBuilders.ValidCreateCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateBarcodeAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            (await CreateSut().UpdateAsync(BarcodeAllocationBuilders.ValidUpdateCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteBarcodeAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            (await CreateSut().DeleteAsync(1)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteBarcodeAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteAsync(1);

            _mockMediator.Verify(m => m.Send(It.IsAny<DeleteBarcodeAllocationCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
