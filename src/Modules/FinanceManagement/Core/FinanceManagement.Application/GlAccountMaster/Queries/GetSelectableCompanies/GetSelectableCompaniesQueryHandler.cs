using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.GlAccountMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetSelectableCompanies
{
    public class GetSelectableCompaniesQueryHandler : IRequestHandler<GetSelectableCompaniesQuery, ApiResponseDTO<List<CompanyOptionDto>>>
    {
        private readonly ICompanyLookup _companyLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetSelectableCompaniesQueryHandler(
            ICompanyLookup companyLookup,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _companyLookup = companyLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CompanyOptionDto>>> Handle(GetSelectableCompaniesQuery request, CancellationToken cancellationToken)
        {
            var userId = _ipAddressService.GetUserId();

            // Profile-based access (AC5): only the companies the user is actively assigned to.
            var companies = await _companyLookup.GetUserCompaniesAsync(userId);
            var options = companies
                .Select(c => new CompanyOptionDto { CompanyId = c.CompanyId, CompanyName = c.CompanyName })
                .ToList();

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetSelectableCompaniesQuery",
                actionCode: "Get",
                actionName: options.Count.ToString(),
                details: "Selectable companies for the account screen were fetched.",
                module: "GlAccountMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<CompanyOptionDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = options,
                TotalCount = options.Count
            };
        }
    }
}
