using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.RawMaterialPO.Commands.CreateRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.DeleteRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.UpdateRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Dto;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetAllRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOAutoComplete;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOById;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOFromOcr;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class RawMaterialPOControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private RawMaterialPOController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllRawMaterialPOQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RawMaterialPODto>>
                {
                    IsSuccess = true,
                    Data = new List<RawMaterialPODto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllRawMaterialPOAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllRawMaterialPOQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RawMaterialPODto>> { IsSuccess = true, Data = new List<RawMaterialPODto>() });

            await CreateSut().GetAllRawMaterialPOAsync(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllRawMaterialPOQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRawMaterialPOByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(RawMaterialPOBuilders.ValidDto());

            var result = await CreateSut().GetRawMaterialPOByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRawMaterialPOAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(RawMaterialPOBuilders.ValidLookupList());

            var result = await CreateSut().GetRawMaterialPOAutoCompleteAsync("RMPO");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task FromOcr_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRawMaterialPOFromOcrQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OcrConversionDto { ConvertedQuantity = 500m, RemainingQuantity = 300m, ConversionStatus = "Partially Converted" });

            var result = await CreateSut().GetRawMaterialPOFromOcrAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateRawMaterialPOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateRawMaterialPO(RawMaterialPOBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateRawMaterialPOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateRawMaterialPO(RawMaterialPOBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRawMaterialPOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteRawMaterialPO(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRawMaterialPOCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteRawMaterialPO(1);

            _mockMediator.Verify(m => m.Send(It.IsAny<DeleteRawMaterialPOCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
