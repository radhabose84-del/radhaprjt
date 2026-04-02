using Contracts.Common;
using InventoryManagement.Application.MRS.Command.CreateMrsEntry;
using InventoryManagement.Application.MRS.Command.UpdateMrsEntry;
using InventoryManagement.Application.MRS.Queries.GetMrsEntry;
using InventoryManagement.Application.MRS.Queries.GetMrsEntryById;
using InventoryManagement.Application.MRS.Queries.GetMrsPending;
using InventoryManagement.Application.MRS.Queries.GetParentWarehouse;
using InventoryManagement.Application.MRS.Queries.GetStockItemBased;
using InventoryManagement.Presentation.Controllers.MRS;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class MrsControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

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
        public async Task CreateAsync_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMrsEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(new CreateMrsEntryCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateMrsEntryCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMrsEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(new UpdateMrsEntryCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMrsEntryDetails_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMrsEntryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMrsEntryDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetMrsEntryDto> { new() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetMrsEntryDetails(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMrsEntryByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetMrsEntryByIdDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetStockAvialble_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStockItemBasedQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetStockItemDto> { new() });

            var result = await CreateSut().GetStockAvialble(1, 1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByParentWarehouseIdAsync_WithExistingId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetParentWarehouseQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetParentWarehouseDto());

            var result = await CreateSut().GetByParentWarehouseIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingMrsAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMrsPendingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MrsPendingDto>>
                {
                    IsSuccess = true,
                    Data = new List<MrsPendingDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetPendingMrsAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
