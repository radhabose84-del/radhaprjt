using Contracts.Common;
using FinanceManagement.Application.CoaReport.Dto;
using FinanceManagement.Application.CoaReport.Queries.GetAccountUsageReport;
using FinanceManagement.Application.CoaReport.Queries.GetCoaListing;
using FinanceManagement.Application.CoaReport.Queries.GetCoaListingPdf;
using FinanceManagement.Application.CoaReport.Queries.GetFsMappingValidation;
using FinanceManagement.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class CoaReportControllerTests
    {
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);
        private CoaReportController CreateSut() => new(_mediator.Object);

        [Fact]
        public async Task GetListing_ReturnsOk()
        {
            _mediator.Setup(m => m.Send(It.IsAny<GetCoaListingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CoaListingItemDto>> { IsSuccess = true, Data = new(), TotalCount = 0 });

            var result = await CreateSut().GetListingAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetListingPdf_ReturnsFile()
        {
            _mediator.Setup(m => m.Send(It.IsAny<GetCoaListingPdfQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReportFileResultDto { Content = new byte[] { 1 }, ContentType = "application/pdf", FileName = "COA_Listing.pdf" });

            var result = await CreateSut().GetListingPdfAsync();
            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).ContentType.Should().Be("application/pdf");
        }

        [Fact]
        public async Task GetAccountUsage_ReturnsOk()
        {
            _mediator.Setup(m => m.Send(It.IsAny<GetAccountUsageReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AccountUsageItemDto>> { IsSuccess = true, Data = new(), TotalCount = 0 });

            var result = await CreateSut().GetAccountUsageAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetFsMappingValidation_ReturnsOk()
        {
            _mediator.Setup(m => m.Send(It.IsAny<GetFsMappingValidationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<FsMappingValidationDto> { IsSuccess = true, Data = new FsMappingValidationDto(), Message = "ok" });

            var result = await CreateSut().GetFsMappingValidationAsync();
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
