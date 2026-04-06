using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetIrnDetails;

namespace FinanceManagement.UnitTests.Application.EInvoiceHeader.Queries
{
    public sealed class GetIrnDetailsQueryHandlerTests
    {
        private readonly Mock<INicEInvoiceService> _mockNicService = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetIrnDetailsQueryHandler CreateSut() =>
            new(_mockNicService.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResultFromNicService()
        {
            var expectedResult = new ApiResponseDTO<object>
            {
                IsSuccess = true,
                Message = "Success",
                Data = new { Irn = "IRN123" }
            };

            _mockNicService
                .Setup(s => s.GetIrnDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var result = await CreateSut().Handle(
                new GetIrnDetailsQuery { EInvoiceHeaderId = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CallsNicServiceOnce()
        {
            var expectedResult = new ApiResponseDTO<object> { IsSuccess = true };
            _mockNicService
                .Setup(s => s.GetIrnDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            await CreateSut().Handle(
                new GetIrnDetailsQuery { EInvoiceHeaderId = 1 }, CancellationToken.None);

            _mockNicService.Verify(
                s => s.GetIrnDetailsAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var expectedResult = new ApiResponseDTO<object> { IsSuccess = true };
            _mockNicService
                .Setup(s => s.GetIrnDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            await CreateSut().Handle(
                new GetIrnDetailsQuery { EInvoiceHeaderId = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "EINVOICE_GET_IRN_DETAILS"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
