using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.ICoaReport;
using FinanceManagement.Application.CoaReport.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaReport.Queries.GetCoaListingPdf
{
    public class GetCoaListingPdfQueryHandler : IRequestHandler<GetCoaListingPdfQuery, ReportFileResultDto>
    {
        private readonly ICoaReportQueryRepository _queryRepository;
        private readonly ICoaListingPdfBuilder _pdfBuilder;
        private readonly ICompanyLookup _companyLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetCoaListingPdfQueryHandler(
            ICoaReportQueryRepository queryRepository,
            ICoaListingPdfBuilder pdfBuilder,
            ICompanyLookup companyLookup,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _pdfBuilder = pdfBuilder;
            _companyLookup = companyLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ReportFileResultDto> Handle(GetCoaListingPdfQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var rows = await _queryRepository.GetCoaListingAsync(
                companyId, request.AccountTypeId, request.AccountGroupId, request.ActiveOnly, request.SearchTerm, cancellationToken);

            var companies = await _companyLookup.GetAllCompanyAsync();
            var companyName = companies.FirstOrDefault(c => c.CompanyId == companyId)?.CompanyName;

            var bytes = _pdfBuilder.Build(companyName, DateTimeOffset.Now, rows);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Export",
                actionCode: "COA_LISTING_PDF",
                actionName: rows.Count.ToString(),
                details: $"COA listing PDF ({rows.Count} accounts) generated for Company {companyId}.",
                module: "CoaReport"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ReportFileResultDto
            {
                Content = bytes,
                FileName = "COA_Listing.pdf",
                ContentType = "application/pdf"
            };
        }
    }
}
