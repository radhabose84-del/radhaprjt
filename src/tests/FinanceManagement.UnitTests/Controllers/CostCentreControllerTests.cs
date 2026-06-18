using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.CostCentre.Commands.CreateCostCentre;
using FinanceManagement.Application.CostCentre.Commands.UpdateCostCentre;
using FinanceManagement.Application.CostCentre.Commands.DeleteCostCentre;
using FinanceManagement.Application.CostCentre.Queries.GetAllCostCentre;
using FinanceManagement.Application.CostCentre.Queries.GetCostCentreById;
using FinanceManagement.Application.CostCentre.Queries.GetCostCentreAutoComplete;
using FinanceManagement.Application.CostCentre.Dto;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class CostCentreControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private CostCentreController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllCostCentreQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CostCentreDto>>
                {
                    IsSuccess = true,
                    Data = new List<CostCentreDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllCostCentreAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCostCentreByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CostCentreDto());

            var result = await CreateSut().GetCostCentreByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCostCentreAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<CostCentreLookupDto>)new List<CostCentreLookupDto>());

            var result = await CreateSut().GetCostCentreAutoCompleteAsync("pro", 59);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCostCentreCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateCostCentre(new CreateCostCentreCommand
            {
                CostCentreCode = "STP",
                CostCentreName = "Plant",
                CentreLevelId = 59
            });
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCostCentreCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateCostCentre(new UpdateCostCentreCommand
            {
                Id = 1,
                CostCentreName = "Plant",
                IsActive = 1
            });
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCostCentreCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteCostCentre(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCostCentreCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteCostCentre(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteCostCentreCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
