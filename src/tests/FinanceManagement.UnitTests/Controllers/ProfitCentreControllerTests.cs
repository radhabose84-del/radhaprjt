using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.ProfitCentre.Commands.CreateProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.UpdateProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.DeleteProfitCentre;
using FinanceManagement.Application.ProfitCentre.Queries.GetAllProfitCentre;
using FinanceManagement.Application.ProfitCentre.Queries.GetProfitCentreById;
using FinanceManagement.Application.ProfitCentre.Queries.GetProfitCentreAutoComplete;
using FinanceManagement.Application.ProfitCentre.Dto;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class ProfitCentreControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ProfitCentreController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllProfitCentreQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ProfitCentreDto>>
                {
                    IsSuccess = true,
                    Data = new List<ProfitCentreDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllProfitCentreAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetProfitCentreByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProfitCentreDto());

            var result = await CreateSut().GetProfitCentreByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetProfitCentreAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ProfitCentreLookupDto>)new List<ProfitCentreLookupDto>());

            var result = await CreateSut().GetProfitCentreAutoCompleteAsync("spin");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateProfitCentreCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateProfitCentre(new CreateProfitCentreCommand
            {
                ProfitCentreCode = "PC-SPIN",
                ProfitCentreName = "Spinning",
                LevelId = 62
            });
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateProfitCentreCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateProfitCentre(new UpdateProfitCentreCommand
            {
                Id = 1,
                ProfitCentreName = "Spinning",
                IsActive = 1
            });
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteProfitCentreCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteProfitCentre(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteProfitCentreCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteProfitCentre(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteProfitCentreCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
