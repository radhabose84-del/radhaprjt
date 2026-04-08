using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.DeletePurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.UpdatePurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentAutoComplete;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentById;
using PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO;
using PurchaseManagement.Presentation.Controllers;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class PurchaseIndentControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<PurchaseIndentController>> _mockLogger = new(MockBehavior.Loose);

        private PurchaseIndentController CreateSut() => new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAllPurchaseIndent_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllPurchaseIndentQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ApiResponseDTO<List<IndentDto>>
                {
                    IsSuccess = true,
                    Data = new List<IndentDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                }));

            var result = await CreateSut().GetAllPurchaseIndentAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllPurchaseIndent_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllPurchaseIndentQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ApiResponseDTO<List<IndentDto>>
                {
                    IsSuccess = true,
                    Data = new List<IndentDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                }));

            await CreateSut().GetAllPurchaseIndentAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllPurchaseIndentQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePurchaseIndentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreatePurchaseIndentCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            var result = await CreateSut().UpdateAsync(new UpdatePurchaseIndentCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_ReturnsOkResult()
        {
            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            var result = await CreateSut().GetByIdAsync(1, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingPurchaseIndent_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingIndentQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ApiResponseDTO<List<PendingIndentDto>>
                {
                    IsSuccess = true,
                    Data = new List<PendingIndentDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                }));

            var result = await CreateSut().GetPendingPurchaseIndentAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingIndentById_ReturnsOkResult()
        {
            var result = await CreateSut().GetPendingIndentByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAutoComplete_ReturnsOkResult()
        {
            var result = await CreateSut().GetPurchaseIndentAutoCompleteAsync("Approved");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetIndentDetailsForPO_ReturnsOkResult()
        {
            var result = await CreateSut().GetIndentDetailsForPOAsync(1, 1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
