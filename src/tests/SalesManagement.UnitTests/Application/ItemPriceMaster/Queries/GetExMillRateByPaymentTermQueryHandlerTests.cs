using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Application.ItemPriceMaster.Queries.GetExMillRateByPaymentTerm;

namespace SalesManagement.UnitTests.Application.ItemPriceMaster.Queries
{
    public class GetExMillRateByPaymentTermQueryHandlerTests
    {
        private readonly Mock<IItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetExMillRateByPaymentTermQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetExMillRateByPaymentTermQueryHandler(
                _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        private static ExMillRateDto ValidDto(int id = 1) => new()
        {
            Id = id,
            PriceCode = "PM001",
            SalesSegmentId = 1,
            SalesSegmentName = "Domestic",
            ExMillRate = 500m,
            CharityValue = 10m,
            HandlingCharges = 5m
        };

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenDataExists()
        {
            var data = new List<ExMillRateDto> { ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetExMillRateByPaymentTermAsync(1, 10, It.IsAny<DateOnly>(), null))
                .ReturnsAsync(data);
            _mockMapper
                .Setup(m => m.Map<List<ExMillRateDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<ExMillRateDto> ?? new List<ExMillRateDto>());

            var query = new GetExMillRateByPaymentTermQuery
            {
                PaymentTermId = 1,
                ItemId = 10,
                Date = DateOnly.FromDateTime(DateTime.Today)
            };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectTotalCount()
        {
            var data = new List<ExMillRateDto> { ValidDto(1), ValidDto(2) };
            _mockQueryRepo
                .Setup(r => r.GetExMillRateByPaymentTermAsync(1, 10, It.IsAny<DateOnly>(), null))
                .ReturnsAsync(data);
            _mockMapper
                .Setup(m => m.Map<List<ExMillRateDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<ExMillRateDto> ?? new List<ExMillRateDto>());

            var query = new GetExMillRateByPaymentTermQuery
            {
                PaymentTermId = 1,
                ItemId = 10,
                Date = DateOnly.FromDateTime(DateTime.Today)
            };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetExMillRateByPaymentTermAsync(null, 10, It.IsAny<DateOnly>(), null))
                .ReturnsAsync(new List<ExMillRateDto>());
            _mockMapper
                .Setup(m => m.Map<List<ExMillRateDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<ExMillRateDto> ?? new List<ExMillRateDto>());

            var query = new GetExMillRateByPaymentTermQuery
            {
                PaymentTermId = null,
                ItemId = 10,
                Date = DateOnly.FromDateTime(DateTime.Today)
            };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepository_Once()
        {
            _mockQueryRepo
                .Setup(r => r.GetExMillRateByPaymentTermAsync(1, 10, It.IsAny<DateOnly>(), 2))
                .ReturnsAsync(new List<ExMillRateDto>());
            _mockMapper
                .Setup(m => m.Map<List<ExMillRateDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<ExMillRateDto> ?? new List<ExMillRateDto>());

            var query = new GetExMillRateByPaymentTermQuery
            {
                PaymentTermId = 1,
                ItemId = 10,
                SalesSegmentId = 2,
                Date = DateOnly.FromDateTime(DateTime.Today)
            };

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetExMillRateByPaymentTermAsync(1, 10, It.IsAny<DateOnly>(), 2),
                Times.Once);
        }
    }
}
