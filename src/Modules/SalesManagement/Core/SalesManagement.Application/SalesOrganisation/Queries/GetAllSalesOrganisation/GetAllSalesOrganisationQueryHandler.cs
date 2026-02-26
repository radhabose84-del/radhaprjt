using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrganisation.Queries.GetAllSalesOrganisation
{
    public class GetAllSalesOrganisationQueryHandler : IRequestHandler<GetAllSalesOrganisationQuery, ApiResponseDTO<List<SalesOrganisationDto>>>
    {
        private readonly ISalesOrganisationQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllSalesOrganisationQueryHandler(ISalesOrganisationQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesOrganisationDto>>> Handle(GetAllSalesOrganisationQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var salesOrganisationDtos = _mapper.Map<List<SalesOrganisationDto>>(data);

            // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesOrganisationQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "SalesOrganisation details were fetched.",
                module: "SalesOrganisation"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesOrganisationDto>>
            {
                IsSuccess = true,
                Message = "Sales Organisations retrieved successfully.",
                Data = salesOrganisationDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
