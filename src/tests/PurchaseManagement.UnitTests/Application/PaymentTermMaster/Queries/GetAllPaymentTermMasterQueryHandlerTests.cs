using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PaymentTermMaster.Queries
{
    public sealed class GetAllPaymentTermMasterQueryHandlerTests
    {
        private readonly Mock<IPaymentTermMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllPaymentTermMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllPaymentTermMasterAsync(1, 10, null))
                .ReturnsAsync((new List<PaymentTermMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllPaymentTermMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithRecords_ReturnsCorrectCount()
        {
            var dtoList = new List<PaymentTermMasterDto>
            {
                PaymentTermMasterBuilders.ValidDto(1),
                PaymentTermMasterBuilders.ValidDto(2)
            };
            _mockQueryRepo
                .Setup(r => r.GetAllPaymentTermMasterAsync(1, 10, null))
                .ReturnsAsync((dtoList, 2));

            var result = await CreateSut().Handle(
                new GetAllPaymentTermMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data!.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllPaymentTermMasterAsync(2, 5, "search"))
                .ReturnsAsync((new List<PaymentTermMasterDto>(), 11));

            var result = await CreateSut().Handle(
                new GetAllPaymentTermMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_WithRecords_ReturnsSuccessMessage()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllPaymentTermMasterAsync(1, 10, null))
                .ReturnsAsync((new List<PaymentTermMasterDto> { PaymentTermMasterBuilders.ValidDto() }, 1));

            var result = await CreateSut().Handle(
                new GetAllPaymentTermMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Handle_CallsGetAllPaymentTermMasterAsyncOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllPaymentTermMasterAsync(1, 10, "PT"))
                .ReturnsAsync((new List<PaymentTermMasterDto>(), 0));

            await CreateSut().Handle(
                new GetAllPaymentTermMasterQuery { PageNumber = 1, PageSize = 10, SearchTerm = "PT" },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetAllPaymentTermMasterAsync(1, 10, "PT"),
                Times.Once);
        }
    }
}
