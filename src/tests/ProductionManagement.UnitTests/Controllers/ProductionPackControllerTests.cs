using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.ProductionPack.Commands.CreateProduction;
using ProductionManagement.Application.ProductionPack.Commands.UpdateProduction;
using ProductionManagement.Application.ProductionPack.Queries.GetAllProduction;
using ProductionManagement.Application.ProductionPack.Queries.GetProductionById;
using ProductionManagement.Application.ProductionPack.Queries.GetProductionAutoComplete;
using ProductionManagement.Application.ProductionPack.Dto;
using ProductionManagement.Presentation.Controllers;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class ProductionPackControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ProductionPackController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllProductionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ProductionPackHeaderDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllProductionAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetProductionByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductionPackHeaderDto { Id = 1 });

            var result = await CreateSut().GetProductionByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetProductionAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductionLookupDto>() as IReadOnlyList<ProductionLookupDto>);

            var result = await CreateSut().GetProductionAutoCompleteAsync(null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateProductionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateProduction(new CreateProductionCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateProductionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateProduction(new UpdateProductionCommand());

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
