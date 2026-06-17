using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxAccountLinkage;
using FinanceManagement.Application.TaxCode.Commands.SubmitLinkageChangeRequest;
using FinanceManagement.Presentation.Controllers;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class TaxAccountLinkageControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private TaxAccountLinkageController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateLinkage_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateTaxAccountLinkageCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateLinkage(new CreateTaxAccountLinkageCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SubmitLinkageChangeRequest_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<SubmitLinkageChangeRequestCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 20 });

            var result = await CreateSut().SubmitLinkageChangeRequest(new SubmitLinkageChangeRequestCommand());
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
