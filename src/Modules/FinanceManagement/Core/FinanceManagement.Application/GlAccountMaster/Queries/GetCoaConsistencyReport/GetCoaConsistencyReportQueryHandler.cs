using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetCoaConsistencyReport
{
    public class GetCoaConsistencyReportQueryHandler : IRequestHandler<GetCoaConsistencyReportQuery, ApiResponseDTO<List<CoaConsistencyReportItemDto>>>
    {
        private readonly IGlAccountMasterQueryRepository _queryRepository;
        private readonly ICompanyLookup _companyLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetCoaConsistencyReportQueryHandler(
            IGlAccountMasterQueryRepository queryRepository,
            ICompanyLookup companyLookup,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _companyLookup = companyLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CoaConsistencyReportItemDto>>> Handle(GetCoaConsistencyReportQuery request, CancellationToken cancellationToken)
        {
            var entityId = _ipAddressService.GetEntityId();

            // The entity group = all companies sharing the session's EntityId.
            var companies = await _companyLookup.GetAllCompanyAsync();
            var groupCompanies = companies.Where(c => c.EntityId == entityId).ToList();
            var companyIds = groupCompanies.Select(c => c.CompanyId).ToList();

            var items = companyIds.Count == 0
                ? new List<CoaConsistencyReportItemDto>()
                : await _queryRepository.GetSingleEntityAccountsAsync(companyIds);

            var nameById = groupCompanies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            foreach (var item in items)
            {
                item.CompanyName = nameById.TryGetValue(item.CompanyId, out var name) ? name : null;
                item.Flag = $"in {item.CompanyName ?? item.CompanyId.ToString()} only";
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetCoaConsistencyReportQuery",
                actionCode: "Get",
                actionName: items.Count.ToString(),
                details: "COA consistency report (single-entity accounts) was fetched.",
                module: "GlAccountMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<CoaConsistencyReportItemDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = items,
                TotalCount = items.Count
            };
        }
    }
}
