using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrderTypeMaster.Queries.GetAllSalesOrderTypeMaster
{
    public class GetAllSalesOrderTypeMasterQueryHandler
        : IRequestHandler<GetAllSalesOrderTypeMasterQuery, ApiResponseDTO<List<SalesOrderTypeMasterDto>>>
    {
        private readonly ISalesOrderTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllSalesOrderTypeMasterQueryHandler(
            ISalesOrderTypeMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesOrderTypeMasterDto>>> Handle(
            GetAllSalesOrderTypeMasterQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm);

            var dtos = _mapper.Map<List<SalesOrderTypeMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesOrderTypeMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "SalesOrderTypeMaster details were fetched.",
                module: "SalesOrderTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesOrderTypeMasterDto>>
            {
                IsSuccess = true,
                Message = "Sales Order Types retrieved successfully",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
