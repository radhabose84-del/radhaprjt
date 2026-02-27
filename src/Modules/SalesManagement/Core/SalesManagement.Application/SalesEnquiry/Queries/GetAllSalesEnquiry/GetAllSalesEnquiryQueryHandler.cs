using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesEnquiry.Queries.GetAllSalesEnquiry
{
    public class GetAllSalesEnquiryQueryHandler : IRequestHandler<GetAllSalesEnquiryQuery, ApiResponseDTO<List<SalesEnquiryHeaderDto>>>
    {
        private readonly ISalesEnquiryQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllSalesEnquiryQueryHandler(
            ISalesEnquiryQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesEnquiryHeaderDto>>> Handle(GetAllSalesEnquiryQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesEnquiryQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Sales Enquiry details were fetched.",
                module: "SalesEnquiry");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesEnquiryHeaderDto>>
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
