using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetEwbDetails;

namespace FinanceManagement.UnitTests.Application.EInvoiceHeader.Queries
{
    public sealed class GetEwbDetailsQueryHandlerTests
    {
        private readonly Mock<INicEInvoiceService> _mockNicService = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetEwbDetailsQueryHandler CreateSut() =>
            new(_mockNicService.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResultFromNicService()
        {
            var expectedResult = new ApiResponseDTO<object>
            {
                IsSuccess = true,
                Message = "Success",
                Data = new { EwbNo = 123456789L }
            };

            _mockNicService
                .Setup(s => s.GetEwbDetailsByIrnAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var result = await CreateSut().Handle(
                new GetEwbDetailsQuery { EInvoiceHeaderId = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CallsNicServiceOnce()
        {
            var expectedResult = new ApiResponseDTO<object> { IsSuccess = true };
            _mockNicService
                .Setup(s => s.GetEwbDetailsByIrnAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            await CreateSut().Handle(
                new GetEwbDetailsQuery { EInvoiceHeaderId = 1 }, CancellationToken.None);

            _mockNicService.Verify(
                s => s.GetEwbDetailsByIrnAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var expectedResult = new ApiResponseDTO<object> { IsSuccess = true };
            _mockNicService
                .Setup(s => s.GetEwbDetailsByIrnAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            await CreateSut().Handle(
                new GetEwbDetailsQuery { EInvoiceHeaderId = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "EINVOICE_GET_EWB_DETAILS"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
