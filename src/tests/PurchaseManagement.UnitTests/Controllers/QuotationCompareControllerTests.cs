using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparison;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonById;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonPending;
using PurchaseManagement.Presentation.Controllers;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class QuotationCompareControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private QuotationCompareController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetComparison_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetQuoteComparisonQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<QuoteComparisonDto?>(new QuoteComparisonDto()));

            var result = await CreateSut().GetComparisonAsync(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetComparison_WhenNotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetQuoteComparisonQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<QuoteComparisonDto?>(null));

            var result = await CreateSut().GetComparisonAsync(999, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateQuoteComparsionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateQuoteComparsionCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetQuoteComparisonPendingQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((new List<QuoteComparisonPendingGroupDto>(), 0)));

            var result = await CreateSut().GetPendingAsync(1, 15, null, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetQuoteComparisonByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new QuoteCompareByIdDto()));

            var result = await CreateSut().GetByIdAsync(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateQuoteComparsionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(new CreateQuoteComparsionCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateQuoteComparsionCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
