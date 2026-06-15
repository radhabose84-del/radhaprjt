using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.AccountTypeMaster.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountTypeMaster.Queries.GetAllAccountTypeMaster
{
    public class GetAllAccountTypeMasterQueryHandler : IRequestHandler<GetAllAccountTypeMasterQuery, ApiResponseDTO<List<AccountTypeMasterDto>>>
    {
        private readonly IAccountTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllAccountTypeMasterQueryHandler(IAccountTypeMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AccountTypeMasterDto>>> Handle(GetAllAccountTypeMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.CompanyId);

            var dtos = _mapper.Map<List<AccountTypeMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllAccountTypeMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "AccountTypeMaster details were fetched.",
                module: "AccountTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<AccountTypeMasterDto>>
            {
                IsSuccess = true,
                Message = "Account Type Master list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
