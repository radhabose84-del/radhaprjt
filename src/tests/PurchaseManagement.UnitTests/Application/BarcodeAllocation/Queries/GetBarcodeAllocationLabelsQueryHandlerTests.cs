using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationLabels;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;

namespace PurchaseManagement.UnitTests.Application.BarcodeAllocation.Queries
{
    public sealed class GetBarcodeAllocationLabelsQueryHandlerTests
    {
        private readonly Mock<IBarcodeAllocationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ICompanyDetailLookup> _mockCompany = new(MockBehavior.Loose);
        private readonly Mock<IDivisionUnitLookup> _mockDivision = new(MockBehavior.Loose);
        private readonly Mock<ICityLookup> _mockCity = new(MockBehavior.Loose);
        private readonly Mock<IStateLookup> _mockState = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetBarcodeAllocationLabelsQueryHandler CreateSut() => new(
            _mockQueryRepo.Object, _mockCompany.Object, _mockDivision.Object, _mockCity.Object,
            _mockState.Object, _mockIp.Object, _mockMediator.Object);

        private static BarcodeAllocationDto Allocation(string prefix = "CTN", long from = 100002198, long to = 100002199) =>
            new()
            {
                Id = 7,
                AllocationNumber = "BBA-2026-0007",
                BarcodeSeriesNumber = "BCS-2026-0007",
                Prefix = prefix,
                BarcodeFrom = from,
                BarcodeTo = to
            };

        private void SetupAllocation(BarcodeAllocationDto dto)
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(dto);
            _mockIp.Setup(s => s.GetUnitId()).Returns(37);
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetDivisionId()).Returns(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((BarcodeAllocationDto?)null);

            var result = await CreateSut().Handle(new GetBarcodeAllocationLabelsQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExpandsRangeWithPrefix()
        {
            SetupAllocation(Allocation("CTN", 100002198, 100002199));

            var result = await CreateSut().Handle(new GetBarcodeAllocationLabelsQuery(7), CancellationToken.None);

            result!.Labels.Should().HaveCount(2);
            result.Labels[0].Barcode.Should().Be("CTN100002198");
            result.Labels[1].Barcode.Should().Be("CTN100002199");
            result.TotalCount.Should().Be(2);
            result.Truncated.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_QrPayloadEqualsBarcode()
        {
            SetupAllocation(Allocation("CTN", 100002198, 100002198));

            var result = await CreateSut().Handle(new GetBarcodeAllocationLabelsQuery(7), CancellationToken.None);

            result!.Labels[0].QrPayload.Should().Be(result.Labels[0].Barcode);
        }

        [Fact]
        public async Task Handle_PopulatesLetterheadFromJwt()
        {
            SetupAllocation(Allocation());
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

            var result = await CreateSut().Handle(new GetBarcodeAllocationLabelsQuery(7), CancellationToken.None);

            result!.Letterhead.CompanyName.Should().Be("BANNARI AMMAN SPINNING MILLS LTD");
            result.Letterhead.DivisionName.Should().Be("Spinning Division");
            result.Letterhead.Address.Should().Be("Trichy Road NH-45, DINDIGUL - 624 802, Tamil Nadu");
        }

        [Fact]
        public async Task Handle_AgentDefaultIsDirect()
        {
            SetupAllocation(Allocation());

            var result = await CreateSut().Handle(new GetBarcodeAllocationLabelsQuery(7), CancellationToken.None);

            result!.AgentDefault.Should().Be("DIRECT");
        }

        [Fact]
        public async Task Handle_LargeRange_TruncatesAtCap()
        {
            SetupAllocation(Allocation("CTN", 1, 10000));

            var result = await CreateSut().Handle(new GetBarcodeAllocationLabelsQuery(7), CancellationToken.None);

            result!.TotalCount.Should().Be(10000);
            result.Labels.Should().HaveCount(5000);
            result.Truncated.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupAllocation(Allocation());

            await CreateSut().Handle(new GetBarcodeAllocationLabelsQuery(7), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
