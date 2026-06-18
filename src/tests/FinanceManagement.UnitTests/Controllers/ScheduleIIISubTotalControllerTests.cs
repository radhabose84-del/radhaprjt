using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;
using FinanceManagement.Application.ScheduleIII.Commands.DeleteSubTotal;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Application.ScheduleIII.Queries.GetSubTotalById;
using FinanceManagement.Application.ScheduleIII.Queries.GetSubTotals;
using FinanceManagement.Presentation.Controllers;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class ScheduleIIISubTotalControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ScheduleIIISubTotalController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSubTotalsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ScheduleIIISubTotalDto>> { IsSuccess = true, Data = new() });

            (await CreateSut().GetAll()).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSubTotalByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ScheduleIIISubTotalDto { Id = 1 });

            (await CreateSut().GetById(1)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOk_AndCallsMediatorOnce()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateSubTotalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().Create(new CreateSubTotalCommand());

            result.Should().BeOfType<OkObjectResult>();
            _mockMediator.Verify(m => m.Send(It.IsAny<CreateSubTotalCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateSubTotalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            (await CreateSut().Update(new UpdateSubTotalCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteSubTotalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            (await CreateSut().Delete(1)).Should().BeOfType<OkObjectResult>();
        }
    }
}
