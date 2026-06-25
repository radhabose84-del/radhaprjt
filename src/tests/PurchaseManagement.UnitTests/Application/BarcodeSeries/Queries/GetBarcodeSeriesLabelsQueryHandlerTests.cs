using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.BarcodeSeries.Dto;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesLabels;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;

namespace PurchaseManagement.UnitTests.Application.BarcodeSeries.Queries
{
    public sealed class GetBarcodeSeriesLabelsQueryHandlerTests
    {
        private readonly Mock<IBarcodeSeriesQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ICompanyDetailLookup> _mockCompany = new(MockBehavior.Loose);
        private readonly Mock<IDivisionUnitLookup> _mockDivision = new(MockBehavior.Loose);
        private readonly Mock<ICityLookup> _mockCity = new(MockBehavior.Loose);
        private readonly Mock<IStateLookup> _mockState = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetBarcodeSeriesLabelsQueryHandler CreateSut() => new(
            _mockQueryRepo.Object, _mockCompany.Object, _mockDivision.Object, _mockCity.Object,
            _mockState.Object, _mockIp.Object, _mockMediator.Object);

        private static BarcodeSeriesDto Series(string prefix = "BLC", long from = 100002211, long to = 100002212) =>
            new()
            {
                Id = 10,
                BarcodeSeriesNumber = "BCS-2026-0010",
                Prefix = prefix,
                BarcodeStartNumber = from,
                BarcodeEndNumber = to
            };

        private void SetupSeries(BarcodeSeriesDto dto)
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(dto);
            _mockIp.Setup(s => s.GetUnitId()).Returns(37);
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetDivisionId()).Returns(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((BarcodeSeriesDto?)null);

            var result = await CreateSut().Handle(new GetBarcodeSeriesLabelsQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExpandsRangeWithPrefix()
        {
            SetupSeries(Series("BLC", 100002211, 100002212));

            var result = await CreateSut().Handle(new GetBarcodeSeriesLabelsQuery(10), CancellationToken.None);

            result!.Labels.Should().HaveCount(2);
            result.Labels[0].Barcode.Should().Be("BLC100002211");
            result.Labels[1].Barcode.Should().Be("BLC100002212");
            result.SeriesNumber.Should().Be("BCS-2026-0010");
            result.TotalCount.Should().Be(2);
            result.Truncated.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_QrPayloadEqualsBarcode()
        {
            SetupSeries(Series("BLC", 100002211, 100002211));

            var result = await CreateSut().Handle(new GetBarcodeSeriesLabelsQuery(10), CancellationToken.None);

            result!.Labels[0].QrPayload.Should().Be(result.Labels[0].Barcode);
        }

        [Fact]
        public async Task Handle_PopulatesLetterheadFromJwt()
        {
            SetupSeries(Series());
            _mockCompany.Setup(c => c.GetByUnitIdAsync(37, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CompanyDetailLookupDto
                {
                    CompanyName = "BANNARI AMMAN SPINNING MILLS LTD",
                    AddressLine1 = "Trichy Road NH-45",
                    CityId = 5, StateId = 9, PinCode = "624 802"
                });
            _mockDivision.Setup(d => d.GetUnitsByDivisionAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DivisionUnitLookupDto>
                {
                    new() { UnitId = 37, DivisionId = 1, DivisionName = "Spinning Division" }
                });
            _mockCity.Setup(c => c.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CityLookupDto { CityId = 5, CityName = "DINDIGUL" });
            _mockState.Setup(s => s.GetByIdAsync(9, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StateLookupDto { StateId = 9, StateName = "Tamil Nadu" });

            var result = await CreateSut().Handle(new GetBarcodeSeriesLabelsQuery(10), CancellationToken.None);

            result!.Letterhead.CompanyName.Should().Be("BANNARI AMMAN SPINNING MILLS LTD");
            result.Letterhead.DivisionName.Should().Be("Spinning Division");
            result.Letterhead.Address.Should().Be("Trichy Road NH-45, DINDIGUL - 624 802, Tamil Nadu");
        }

        [Fact]
        public async Task Handle_AgentDefaultIsDirect()
        {
            SetupSeries(Series());

            var result = await CreateSut().Handle(new GetBarcodeSeriesLabelsQuery(10), CancellationToken.None);

            result!.AgentDefault.Should().Be("DIRECT");
        }

        [Fact]
        public async Task Handle_LargeRange_TruncatesAtCap()
        {
            SetupSeries(Series("BLC", 1, 10000));

            var result = await CreateSut().Handle(new GetBarcodeSeriesLabelsQuery(10), CancellationToken.None);

            result!.TotalCount.Should().Be(10000);
            result.Labels.Should().HaveCount(5000);
            result.Truncated.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupSeries(Series());

            await CreateSut().Handle(new GetBarcodeSeriesLabelsQuery(10), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
