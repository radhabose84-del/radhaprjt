using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.MRS.Command.CreateMrsEntry;
using PurchaseManagement.Application.MRS.Command.UpdateMrsEntry;
using PurchaseManagement.Application.MRS.Queries.GetMrsEntry;
using PurchaseManagement.Application.MRS.Queries.GetMrsEntryById;
using PurchaseManagement.Application.MRS.Queries.GetMrsPending;
using PurchaseManagement.Application.MRS.Queries.GetParentWarehouse;
using PurchaseManagement.Application.MRS.Queries.GetStockItemBased;
using PurchaseManagement.Presentation.Controllers.MRS;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class MrsControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private MrsController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMrsEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateMrsEntryCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            var result = await CreateSut().UpdateAsync(new UpdateMrsEntryCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMrsEntryDetails_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMrsEntryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<object>>
                {
                    Data = new List<object>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetMrsEntryDetails(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMrsEntryByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new object());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMrsEntryByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((object?)null);

            var result = await CreateSut().GetByIdAsync(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetStockAvialble_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStockItemBasedQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object> { new object() });

            var result = await CreateSut().GetStockAvialble(1, 1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetStockAvialble_WhenEmpty_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStockItemBasedQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object>());

            var result = await CreateSut().GetStockAvialble(1, 1, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetByParentWarehouseIdAsync_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetParentWarehouseQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new object());

            var result = await CreateSut().GetByParentWarehouseIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingMrs_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMrsPendingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<object>>
                {
                    Data = new List<object>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetPendingMrsAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMrsEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(new CreateMrsEntryCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateMrsEntryCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
