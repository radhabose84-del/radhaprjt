using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Application.ScheduleIII.Commands.CreateLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;
using FinanceManagement.Application.ScheduleIII.Commands.DeleteLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.LockStructure;
using FinanceManagement.Application.ScheduleIII.Commands.ReorderLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Application.ScheduleIII.Queries.GetStructure;
using FinanceManagement.Application.ScheduleIII.Queries.GetSubTotals;
using FinanceManagement.Presentation.Controllers;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class ScheduleIIIControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ScheduleIIIController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetStructure_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetStructureQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ScheduleIIIStructureDto { Id = 1 });

            var result = await CreateSut().GetStructureAsync(1001, 7);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetSubTotals_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSubTotalsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ScheduleIIISubTotalDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetSubTotalsAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateLineItem_ReturnsOk_AndCallsMediatorOnce()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateLineItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateLineItem(new CreateLineItemCommand());

            result.Should().BeOfType<OkObjectResult>();
            _mockMediator.Verify(m => m.Send(It.IsAny<CreateLineItemCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateLineItem_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateLineItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 14 });

            var result = await CreateSut().UpdateLineItem(new UpdateLineItemCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteLineItem_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteLineItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteLineItem(14);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ReorderLineItem_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<ReorderLineItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true, Data = true });

            var result = await CreateSut().ReorderLineItem(new ReorderLineItemCommand { LineItemId = 14, Direction = 1 });
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateSubTotal_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateSubTotalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateSubTotal(new CreateSubTotalCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateSubTotal_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateSubTotalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 2 });

            var result = await CreateSut().UpdateSubTotal(new UpdateSubTotalCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task LockStructure_ReturnsOk_AndCallsMediatorOnce()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<LockStructureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true, Data = true });

            var result = await CreateSut().LockStructure(new LockStructureCommand { StructureId = 1 });

            result.Should().BeOfType<OkObjectResult>();
            _mockMediator.Verify(m => m.Send(It.IsAny<LockStructureCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
