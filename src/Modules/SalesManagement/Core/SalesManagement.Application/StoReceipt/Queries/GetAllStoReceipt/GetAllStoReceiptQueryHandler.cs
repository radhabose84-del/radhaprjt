using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoReceipt.Queries.GetAllStoReceipt
{
    public class GetAllStoReceiptQueryHandler : IRequestHandler<GetAllStoReceiptQuery, ApiResponseDTO<List<StoReceiptHeaderDto>>>
    {
        private readonly IStoReceiptQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllStoReceiptQueryHandler(
            IStoReceiptQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<StoReceiptHeaderDto>>> Handle(GetAllStoReceiptQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllStoReceiptQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "STO Receipt details were fetched.",
                module: "StoReceipt");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<StoReceiptHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
