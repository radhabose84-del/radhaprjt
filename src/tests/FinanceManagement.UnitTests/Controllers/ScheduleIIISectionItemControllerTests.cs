using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Application.ScheduleIII.Commands.CreateLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.DeleteLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateLineItem;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Application.ScheduleIII.Queries.GetAllLineItem;
using FinanceManagement.Application.ScheduleIII.Queries.GetLineItemById;
using FinanceManagement.Presentation.Controllers;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class ScheduleIIISectionItemControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ScheduleIIISectionItemController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllLineItemQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ScheduleIIISectionItemDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetAll(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetLineItemByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ScheduleIIISectionItemDto { Id = 14 });

            var result = await CreateSut().GetById(14);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOk_AndCallsMediatorOnce()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateLineItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().Create(new CreateLineItemCommand());

            result.Should().BeOfType<OkObjectResult>();
            _mockMediator.Verify(m => m.Send(It.IsAny<CreateLineItemCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateLineItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 14 });

            var result = await CreateSut().Update(new UpdateLineItemCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteLineItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(14);
            result.Should().BeOfType<OkObjectResult>();
        }

    }
}
