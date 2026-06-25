using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.ICoaReport;
using FinanceManagement.Application.CoaReport.Dto;
using FinanceManagement.Application.CoaReport.Queries.GetAccountUsageReport;
using FinanceManagement.Application.CoaReport.Queries.GetCoaListing;
using FinanceManagement.Application.CoaReport.Queries.GetCoaListingPdf;
using FinanceManagement.Application.CoaReport.Queries.GetFsMappingValidation;

namespace FinanceManagement.UnitTests.Application.CoaReport
{
    public sealed class CoaReportQueryHandlerTests
    {
        private readonly Mock<ICoaReportQueryRepository> _repo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        public CoaReportQueryHandlerTests() => _ip.Setup(s => s.GetCompanyId()).Returns(1);

        // ── Listing (AC1) ─────────────────────────────────────────────────────────
        [Fact]
        public async Task Listing_ReturnsRows_ForSessionCompany()
        {
            _repo.Setup(r => r.GetCoaListingAsync(1, null, null, false, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoaListingItemDto> { new() { AccountCode = "1001" } });

            var sut = new GetCoaListingQueryHandler(_repo.Object, _ip.Object, _mediator.Object);
            var result = await sut.Handle(new GetCoaListingQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Listing_NoActiveCompany_Throws()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns((int?)null);
            var sut = new GetCoaListingQueryHandler(_repo.Object, _ip.Object, _mediator.Object);

            var act = async () => await sut.Handle(new GetCoaListingQuery(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*active company*");
        }

        // ── PDF (AC1/AC5) ─────────────────────────────────────────────────────────
        [Fact]
        public async Task Pdf_BuildsFromRows_AndReturnsPdfFile()
        {
            var rows = new List<CoaListingItemDto> { new() { AccountCode = "1001", AccountName = "Cash" } };
            _repo.Setup(r => r.GetCoaListingAsync(1, null, null, false, null, It.IsAny<CancellationToken>())).ReturnsAsync(rows);

            var lookup = new Mock<ICompanyLookup>(MockBehavior.Loose);
            lookup.Setup(c => c.GetAllCompanyAsync()).ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Acme" } });

            var builder = new Mock<ICoaListingPdfBuilder>(MockBehavior.Loose);
            builder.Setup(b => b.Build(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), rows)).Returns(new byte[] { 1, 2, 3 });

            var sut = new GetCoaListingPdfQueryHandler(_repo.Object, builder.Object, lookup.Object, _ip.Object, _mediator.Object);
            var file = await sut.Handle(new GetCoaListingPdfQuery(), CancellationToken.None);

            file.ContentType.Should().Be("application/pdf");
            file.FileName.Should().Be("COA_Listing.pdf");
            file.Content.Should().NotBeEmpty();
            builder.Verify(b => b.Build("Acme", It.IsAny<DateTimeOffset>(), rows), Times.Once);
        }

        // ── Usage (AC2/AC3) ───────────────────────────────────────────────────────
        [Fact]
        public async Task Usage_DefaultsTo12Months_AndPassesThrough()
        {
            _repo.Setup(r => r.GetAccountUsageAsync(1, 12, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AccountUsageItemDto> { new() { AccountCode = "5001", IsDeactivationCandidate = true } });

            var sut = new GetAccountUsageReportQueryHandler(_repo.Object, _ip.Object, _mediator.Object);
            var result = await sut.Handle(new GetAccountUsageReportQuery(), CancellationToken.None);

            result.Data.Should().HaveCount(1);
            _repo.Verify(r => r.GetAccountUsageAsync(1, 12, It.IsAny<CancellationToken>()), Times.Once);
        }

        // ── FS-mapping (AC4) ──────────────────────────────────────────────────────
        [Fact]
        public async Task FsMapping_Clean_ReturnsCleanMessage()
        {
            _repo.Setup(r => r.GetFsMappingValidationAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FsMappingValidationDto { TotalLeafGroups = 5, MappedCount = 5, UnmappedCount = 0 });

            var sut = new GetFsMappingValidationQueryHandler(_repo.Object, _ip.Object, _mediator.Object);
            var result = await sut.Handle(new GetFsMappingValidationQuery(), CancellationToken.None);

            result.Data!.IsClean.Should().BeTrue();
            result.Message.Should().Contain("mapped");
        }

        [Fact]
        public async Task FsMapping_WithUnmapped_ReportsCount()
        {
            _repo.Setup(r => r.GetFsMappingValidationAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FsMappingValidationDto
                {
                    TotalLeafGroups = 5, MappedCount = 3, UnmappedCount = 2,
                    Unmapped = new() { new() { GroupCode = "A" }, new() { GroupCode = "B" } }
                });

            var sut = new GetFsMappingValidationQueryHandler(_repo.Object, _ip.Object, _mediator.Object);
            var result = await sut.Handle(new GetFsMappingValidationQuery(), CancellationToken.None);

            result.Data!.IsClean.Should().BeFalse();
            result.Data!.UnmappedCount.Should().Be(2);
            result.Message.Should().Contain("2");
        }
    }
}
