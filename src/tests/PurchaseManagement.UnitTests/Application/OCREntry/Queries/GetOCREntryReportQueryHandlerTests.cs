using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryReport;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.OCREntry.Queries
{
    public sealed class GetOCREntryReportQueryHandlerTests
    {
        private readonly Mock<IOCREntryQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ICompanyDetailLookup> _mockCompany = new(MockBehavior.Loose);
        private readonly Mock<IUnitDetailLookup> _mockUnit = new(MockBehavior.Loose);
        private readonly Mock<ICityLookup> _mockCity = new(MockBehavior.Loose);
        private readonly Mock<IStateLookup> _mockState = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflow = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetOCREntryReportQueryHandler CreateSut() => new(
            _mockQueryRepo.Object, _mockCompany.Object, _mockUnit.Object, _mockCity.Object,
            _mockState.Object, _mockWorkflow.Object, _mockIp.Object, _mockMediator.Object);

        private void SetupOcr(OCREntryDto dto)
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(dto);
            _mockIp.Setup(s => s.GetUnitId()).Returns(37);
        }

        private static OcrReportField? Find(OcrReportDto report, string key) =>
            report.Sections.SelectMany(s => s.Fields).FirstOrDefault(f => f.Key == key);

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((OCREntryDto?)null);

            var result = await CreateSut().Handle(new GetOCREntryReportQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_Valid_ReturnsExpectedSections()
        {
            SetupOcr(OCREntryBuilders.ValidDto(1));

            var result = await CreateSut().Handle(new GetOCREntryReportQuery(1), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Sections.Select(s => s.Key).Should().ContainInOrder(
                "company", "documentIdentity", "orderDetails", "cottonParameters", "freight", "footer");
        }

        [Fact]
        public async Task Handle_Valid_FormatsQuantityWithBalesLabel()
        {
            SetupOcr(OCREntryBuilders.ValidDto(1));

            var result = await CreateSut().Handle(new GetOCREntryReportQuery(1), CancellationToken.None);

            var quantity = Find(result!, "quantity");
            quantity!.Value.Should().Be("100 Bales");
            quantity.Raw.Should().Be(100m);
        }

        [Fact]
        public async Task Handle_Valid_BuildsRateCandyComposite()
        {
            var dto = OCREntryBuilders.ValidDto(1);
            dto.Rate = 365m;
            dto.GstPercentage = 5m;
            dto.ProcurementTypeName = "Spot";
            SetupOcr(dto);

            var result = await CreateSut().Handle(new GetOCREntryReportQuery(1), CancellationToken.None);

            Find(result!, "rateCandy")!.Value.Should().Be("Rs.365.00 + 5.00% GST Spot");
        }

        [Fact]
        public async Task Handle_NoUom_RateLabelFallsBackToCandy()
        {
            var dto = OCREntryBuilders.ValidDto(1);
            dto.UomName = null;
            SetupOcr(dto);

            var result = await CreateSut().Handle(new GetOCREntryReportQuery(1), CancellationToken.None);

            Find(result!, "rateCandy")!.Label.Should().Be("Rate/Candy");
        }

        [Fact]
        public async Task Handle_PopulatesLetterheadAndLogo()
        {
            SetupOcr(OCREntryBuilders.ValidDto(1));
            _mockCompany.Setup(c => c.GetByUnitIdAsync(37, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CompanyDetailLookupDto
                {
                    CompanyName = "BASML",
                    GstNumber = "33AAACB8513A1ZE",
                    LogoUrl = "http://192.168.1.126/bsofterp/Resources/AllFiles/logo.png"
                });
            _mockUnit.Setup(u => u.GetByIdAsync(37, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitDetailLookupDto { UnitName = "UNIT-II", CINNO = "L17111TZ1989PLC002476" });

            var result = await CreateSut().Handle(new GetOCREntryReportQuery(1), CancellationToken.None);

            Find(result!, "companyName")!.Value.Should().Be("BASML");
            Find(result!, "unit")!.Value.Should().Be("UNIT-II");
            Find(result!, "cin")!.Value.Should().Be("L17111TZ1989PLC002476");
            Find(result!, "gstin")!.Value.Should().Be("33AAACB8513A1ZE");
            Find(result!, "logo")!.Value.Should().Be("http://192.168.1.126/bsofterp/Resources/AllFiles/logo.png");
        }

        [Fact]
        public async Task Handle_ReadsLogoBase64FromCompanyLogoFile()
        {
            SetupOcr(OCREntryBuilders.ValidDto(1));
            var tempFile = Path.GetTempFileName();
            var bytes = new byte[] { 1, 2, 3, 4, 5 };
            await File.WriteAllBytesAsync(tempFile, bytes);
            try
            {
                _mockCompany.Setup(c => c.GetByUnitIdAsync(37, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CompanyDetailLookupDto { Logo = tempFile });

                var result = await CreateSut().Handle(new GetOCREntryReportQuery(1), CancellationToken.None);

                Find(result!, "logoBase64")!.Value.Should().Be(Convert.ToBase64String(bytes));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task Handle_NoLogoFile_LeavesLogoBase64Blank()
        {
            SetupOcr(OCREntryBuilders.ValidDto(1));

            var result = await CreateSut().Handle(new GetOCREntryReportQuery(1), CancellationToken.None);

            Find(result!, "logoBase64")!.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NotApproved_LeavesApprovalFieldsBlank()
        {
            SetupOcr(OCREntryBuilders.ValidDto(1));
            _mockWorkflow.Setup(w => w.GetApprovalInfoAsync(It.IsAny<string>(), 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ApprovalInfoDto?)null);

            var result = await CreateSut().Handle(new GetOCREntryReportQuery(1), CancellationToken.None);

            Find(result!, "cottonApprovedBy")!.Value.Should().BeEmpty();
            Find(result!, "cottonApprovedOn")!.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_FreightPresent_FormatsMoney()
        {
            SetupOcr(OCREntryBuilders.ValidDto(1));
            _mockQueryRepo.Setup(r => r.GetFreightForOcrAsync(1)).ReturnsAsync((50m, 50000m));

            var result = await CreateSut().Handle(new GetOCREntryReportQuery(1), CancellationToken.None);

            Find(result!, "freightPerBale")!.Value.Should().Be("Rs.50.00");
            Find(result!, "freightTotal")!.Value.Should().Be("Rs.50,000.00");
        }

        [Fact]
        public async Task Handle_NoFreight_LeavesFreightBlank()
        {
            SetupOcr(OCREntryBuilders.ValidDto(1));
            _mockQueryRepo.Setup(r => r.GetFreightForOcrAsync(1)).ReturnsAsync(((decimal?)null, (decimal?)null));

            var result = await CreateSut().Handle(new GetOCREntryReportQuery(1), CancellationToken.None);

            Find(result!, "freightPerBale")!.Value.Should().BeEmpty();
            Find(result!, "freightTotal")!.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Valid_PublishesAuditEvent()
        {
            SetupOcr(OCREntryBuilders.ValidDto(1));

            await CreateSut().Handle(new GetOCREntryReportQuery(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
