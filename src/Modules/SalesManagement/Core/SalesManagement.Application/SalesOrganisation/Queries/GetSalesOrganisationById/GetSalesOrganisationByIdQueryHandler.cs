#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrganisation.Queries.GetSalesOrganisationById
{
    public class GetSalesOrganisationByIdQueryHandler : IRequestHandler<GetSalesOrganisationByIdQuery, ApiResponseDTO<SalesOrganisationDto>>
    {
        private readonly ISalesOrganisationQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesOrganisationByIdQueryHandler(ISalesOrganisationQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<SalesOrganisationDto>> Handle(GetSalesOrganisationByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var salesOrganisation = _mapper.Map<SalesOrganisationDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesOrganisationByIdQuery",
                actionName: salesOrganisation.Id.ToString(),
                details: $"SalesOrganisation details {salesOrganisation.Id} was fetched.",
                module: "SalesOrganisation"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<SalesOrganisationDto>
            {
                IsSuccess = true,
                Message = "Sales Organisation retrieved successfully.",
                Data = salesOrganisation
            };
        }
    }
}
