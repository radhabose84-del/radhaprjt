using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Application.ScheduleIII.Commands.SaveSubTotalFormula;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Application.ScheduleIII.Queries.GetSubTotalFormulaOperands;
using FinanceManagement.Presentation.Controllers;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class ScheduleIIISubTotalFormulaControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ScheduleIIISubTotalFormulaController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetFormulaOperands_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSubTotalFormulaOperandsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<SubTotalFormulaOperandDto>> { IsSuccess = true, Data = new() });

            (await CreateSut().GetFormulaOperands(2)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOk_AndCallsMediatorOnce()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<SaveSubTotalFormulaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 2 });

            var result = await CreateSut().Create(new SaveSubTotalFormulaCommand());

            result.Should().BeOfType<OkObjectResult>();
            _mockMediator.Verify(m => m.Send(It.IsAny<SaveSubTotalFormulaCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<SaveSubTotalFormulaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 2 });

            (await CreateSut().Update(new SaveSubTotalFormulaCommand())).Should().BeOfType<OkObjectResult>();
        }
    }
}
