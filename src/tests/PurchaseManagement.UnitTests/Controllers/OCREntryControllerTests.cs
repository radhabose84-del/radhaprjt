using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.OCREntry.Commands.CreateOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.DeleteOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.UpdateOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Application.OCREntry.Queries.GetAllOCREntry;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryAutoComplete;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryById;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryPending;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryReport;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class OCREntryControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private OCREntryController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllOCREntryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<OCREntryDto>>
                {
                    IsSuccess = true,
                    Data = new List<OCREntryDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllOCREntryAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllOCREntryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<OCREntryDto>> { IsSuccess = true, Data = new List<OCREntryDto>() });

            await CreateSut().GetAllOCREntryAsync(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllOCREntryQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetOCREntryByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(OCREntryBuilders.ValidDto());

            var result = await CreateSut().GetOCREntryByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetOCREntryAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OCREntryLookupDto>());

            var result = await CreateSut().GetOCREntryAutoCompleteAsync("OCR");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetReport_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetOCREntryReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OcrReportDto());

            var result = await CreateSut().GetOCREntryReportAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Pending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetOCREntryPendingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<OCREntryDto>> { IsSuccess = true, Data = new List<OCREntryDto>() });

            var result = await CreateSut().GetOCREntryPendingAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateOCREntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateOCREntry(OCREntryBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateOCREntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateOCREntry(OCREntryBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteOCREntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteOCREntry(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteOCREntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteOCREntry(1);

            _mockMediator.Verify(m => m.Send(It.IsAny<DeleteOCREntryCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
