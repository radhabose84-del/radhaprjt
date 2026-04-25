using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.DeliveryChallan.Commands.CreateDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Commands.DeleteDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Commands.GenerateEWaybillForDC;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Application.DeliveryChallan.Queries.GetAllDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanAutoComplete;
using SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanById;
using SalesManagement.Application.DeliveryChallan.Queries.GetDCGatePassPending;
using SalesManagement.Application.DeliveryChallan.Queries.GetPendingDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Queries.GetPendingDeliveryChallanById;
using SalesManagement.Application.DeliveryChallan.Queries.GetStoOpenQty;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers
{
    public sealed class DeliveryChallanControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeliveryChallanController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllDeliveryChallanQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DeliveryChallanHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<DeliveryChallanHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllDeliveryChallanAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDeliveryChallanByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeliveryChallanHeaderDto?)null);

            var result = await CreateSut().GetDeliveryChallanByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDeliveryChallanAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DeliveryChallanLookupDto>() as IReadOnlyList<DeliveryChallanLookupDto>);

            var result = await CreateSut().GetDeliveryChallanAutoCompleteAsync("test");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingDeliveryChallanQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DeliveryChallanHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<DeliveryChallanHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetPendingDeliveryChallanAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingDeliveryChallanByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PendingDeliveryChallanByIdDto?)null);

            var result = await CreateSut().GetPendingDeliveryChallanByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetStoOpenQty_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStoOpenQtyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((StoOpenQtyDto?)null);

            var result = await CreateSut().GetStoOpenQtyAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetDCGatePassPending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDCGatePassPendingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetDCGatePassPendingDto>());

            var result = await CreateSut().GetDCGatePassPendingAsync("KA01");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDeliveryChallanCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateDeliveryChallan(new CreateDeliveryChallanCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDeliveryChallanCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteDeliveryChallan(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDeliveryChallanCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            await CreateSut().CreateDeliveryChallan(new CreateDeliveryChallanCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateDeliveryChallanCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateEWaybill_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GenerateEWaybillForDCCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GenerateEWaybillResponseDto>
                {
                    IsSuccess = true,
                    Message = "ok",
                    Data = new GenerateEWaybillResponseDto
                    {
                        EWaybillHeaderId = 42,
                        DeliveryNumber = "DC-2026-0001",
                        EwbStatus = "Pending",
                        AlreadyExisted = false
                    }
                });

            var result = await CreateSut().GenerateEWaybillAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GenerateEWaybill_CallsMediatorSend_WithDcId()
        {
            GenerateEWaybillForDCCommand? captured = null;
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GenerateEWaybillForDCCommand>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<ApiResponseDTO<GenerateEWaybillResponseDto>>, CancellationToken>(
                    (cmd, _) => captured = (GenerateEWaybillForDCCommand)cmd)
                .ReturnsAsync(new ApiResponseDTO<GenerateEWaybillResponseDto>
                {
                    IsSuccess = true,
                    Data = new GenerateEWaybillResponseDto { EWaybillHeaderId = 7 }
                });

            await CreateSut().GenerateEWaybillAsync(123);

            captured.Should().NotBeNull();
            captured!.DeliveryChallanId.Should().Be(123);
            _mockMediator.Verify(
                m => m.Send(It.IsAny<GenerateEWaybillForDCCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
