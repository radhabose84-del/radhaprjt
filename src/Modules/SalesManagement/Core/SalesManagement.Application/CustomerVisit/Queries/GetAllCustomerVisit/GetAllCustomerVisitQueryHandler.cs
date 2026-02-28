using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.CustomerVisit.Queries.GetAllCustomerVisit
{
    public class GetAllCustomerVisitQueryHandler : IRequestHandler<GetAllCustomerVisitQuery, ApiResponseDTO<List<CustomerVisitDto>>>
    {
        private readonly ICustomerVisitQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllCustomerVisitQueryHandler(ICustomerVisitQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CustomerVisitDto>>> Handle(GetAllCustomerVisitQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<CustomerVisitDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllCustomerVisitQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "CustomerVisit details were fetched.",
                module: "CustomerVisit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<CustomerVisitDto>>
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
