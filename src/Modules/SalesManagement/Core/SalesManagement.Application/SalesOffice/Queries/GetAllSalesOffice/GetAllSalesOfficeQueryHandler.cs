using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOffice.Queries.GetAllSalesOffice
{
    public class GetAllSalesOfficeQueryHandler : IRequestHandler<GetAllSalesOfficeQuery, ApiResponseDTO<List<SalesOfficeDto>>>
    {
        private readonly ISalesOfficeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllSalesOfficeQueryHandler(ISalesOfficeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesOfficeDto>>> Handle(GetAllSalesOfficeQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var salesOfficeDtos = _mapper.Map<List<SalesOfficeDto>>(data);

            // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesOfficeQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "SalesOffice details were fetched.",
                module: "SalesOffice"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesOfficeDto>>
            {
                IsSuccess = true,
                Message = "Sales Offices retrieved successfully.",
                Data = salesOfficeDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
