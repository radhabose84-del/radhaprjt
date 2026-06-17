using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetAllGstrSectionMaster
{
    public class GetAllGstrSectionMasterQueryHandler : IRequestHandler<GetAllGstrSectionMasterQuery, ApiResponseDTO<List<GstrSectionMasterDto>>>
    {
        private readonly IGstrSectionQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetAllGstrSectionMasterQueryHandler(IGstrSectionQueryRepository queryRepository, IIPAddressService ipAddressService, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GstrSectionMasterDto>>> Handle(GetAllGstrSectionMasterQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetAllSectionsAsync(request.PageNumber, request.PageSize, request.SearchTerm, companyId, request.ReportTypeId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllGstrSectionMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "GSTR section master details were fetched.",
                module: "GstrSectionMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GstrSectionMasterDto>>
            {
                IsSuccess = true,
                Message = "GSTR section master list retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
