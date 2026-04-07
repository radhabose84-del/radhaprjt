using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Update;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetVendorServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePOPending;
using PurchaseManagement.Presentation.Controllers.PurchaseOrder;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class ServicePurchaseOrderControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Loose);

        private ServicePurchaseOrderController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateServicePo_WhenSuccessful_ReturnsCreatedResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateServicePoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateServicePo(new CreateServicePurchaseOrderDto(), CancellationToken.None);

            result.Should().BeOfType<CreatedAtRouteResult>();
        }

        [Fact]
        public async Task CreateServicePo_WhenFails_ReturnsBadRequest()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateServicePoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await CreateSut().CreateServicePo(new CreateServicePurchaseOrderDto(), CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetById_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetServicePOByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new object());

            var result = await CreateSut().GetById(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_WhenNotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetServicePOByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((object?)null);

            var result = await CreateSut().GetById(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetVendorServicePO_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetVendorServicePOQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new object());

            var result = await CreateSut().GetVendorServicePO(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetVendorServicePO_WhenNull_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetVendorServicePOQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((object?)null);

            var result = await CreateSut().GetVendorServicePO(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetPendingServicePO_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPOServicePendingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<object>(), 0));

            var result = await CreateSut().GetPendingServicePOAsync();

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
