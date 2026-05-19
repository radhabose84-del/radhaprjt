using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Update;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetAllPurchaseOrder;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderById;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderAutocomplete;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPOLocalPending;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Presentation.Controllers.PurchaseOrder;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class PurchaseOrderLocalControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Loose);

        private PurchaseOrderLocalController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePurchaseOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().Create(new PurchaseOrderCreateDto(), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePurchaseOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(new PurchaseOrderUpdateDto(), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPurchaseOrdersQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PagedResult<PurchaseOrderListItemDto>()));

            var result = await CreateSut().GetAll(1, 20, null, null, null, null, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPurchaseOrderByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<PurchaseOrderDetailDto?>(new PurchaseOrderDetailDto()));

            var result = await CreateSut().GetById(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Autocomplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPurchaseOrderAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<AutocompleteDto>>(new List<AutocompleteDto>()));

            var result = await CreateSut().Autocomplete("test", null, null, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePurchaseOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            await CreateSut().Create(new PurchaseOrderCreateDto(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreatePurchaseOrderCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
