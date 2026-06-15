using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.AccessPolicy.Dto;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;

namespace UserManagement.Application.AccessPolicy.Queries.GetAllAccessPolicy
{
    public class GetAllAccessPolicyQueryHandler
        : IRequestHandler<GetAllAccessPolicyQuery, ApiResponseDTO<List<AccessPolicyDto>>>
    {
        private readonly IAccessPolicyQueryRepository _queryRepository;
        private readonly IMapper                      _mapper;
        private readonly IMediator                    _mediator;

        public GetAllAccessPolicyQueryHandler(
            IAccessPolicyQueryRepository queryRepository,
            IMapper                      mapper,
            IMediator                    mediator)
        {
            _queryRepository = queryRepository;
            _mapper          = mapper;
            _mediator        = mediator;
        }

        public async Task<ApiResponseDTO<List<AccessPolicyDto>>> Handle(
            GetAllAccessPolicyQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllAccessPolicyQuery",
                actionCode:   "Get",
                actionName:   data.Count.ToString(),
                details:      "AccessPolicy details were fetched.",
                module:       "AccessPolicy"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<AccessPolicyDto>>
            {
                IsSuccess  = true,
                Message    = "Success",
                Data       = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize   = request.PageSize
            };
        }
    }
}
