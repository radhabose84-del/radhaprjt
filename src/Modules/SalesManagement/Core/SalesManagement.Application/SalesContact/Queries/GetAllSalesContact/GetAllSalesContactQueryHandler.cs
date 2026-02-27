using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.SalesContact.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesContact.Queries.GetAllSalesContact
{
    public class GetAllSalesContactQueryHandler : IRequestHandler<GetAllSalesContactQuery, ApiResponseDTO<List<SalesContactDto>>>
    {
        private readonly ISalesContactQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllSalesContactQueryHandler(
            ISalesContactQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesContactDto>>> Handle(GetAllSalesContactQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<SalesContactDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesContactQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Sales Contact details were fetched.",
                module: "SalesContact"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesContactDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
