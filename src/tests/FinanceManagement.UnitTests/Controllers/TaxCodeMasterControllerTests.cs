using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeMaster;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Application.TaxCode.Queries.GetAllTaxCodeMaster;
using FinanceManagement.Presentation.Controllers;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class TaxCodeMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private TaxCodeMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllTaxCodes_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllTaxCodeMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<TaxCodeMasterDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllTaxCodesAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateTaxCode_ReturnsOk_AndCallsMediatorOnce()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateTaxCodeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateTaxCode(new CreateTaxCodeMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
            _mockMediator.Verify(m => m.Send(It.IsAny<CreateTaxCodeMasterCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
