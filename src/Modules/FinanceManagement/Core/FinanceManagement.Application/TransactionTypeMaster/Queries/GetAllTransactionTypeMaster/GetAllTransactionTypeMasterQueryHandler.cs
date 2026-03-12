using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TransactionTypeMaster.Queries.GetAllTransactionTypeMaster
{
    public class GetAllTransactionTypeMasterQueryHandler : IRequestHandler<GetAllTransactionTypeMasterQuery, ApiResponseDTO<List<TransactionTypeMasterDto>>>
    {
        private readonly ITransactionTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllTransactionTypeMasterQueryHandler(
            ITransactionTypeMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<TransactionTypeMasterDto>>> Handle(GetAllTransactionTypeMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<TransactionTypeMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllTransactionTypeMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Transaction Type Master details were fetched.",
                module: "TransactionTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<TransactionTypeMasterDto>>
            {
                IsSuccess = true,
                Message = "Transaction Type Masters retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
