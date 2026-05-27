using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorRatingGrade.Queries.GetAllVendorRatingGrade
{
    public class GetAllVendorRatingGradeQueryHandler : IRequestHandler<GetAllVendorRatingGradeQuery, ApiResponseDTO<List<VendorRatingGradeDto>>>
    {
        private readonly IVendorRatingGradeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllVendorRatingGradeQueryHandler(IVendorRatingGradeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<VendorRatingGradeDto>>> Handle(GetAllVendorRatingGradeQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<VendorRatingGradeDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllVendorRatingGradeQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "VendorRatingGrade details were fetched.",
                module: "VendorRatingGrade"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<VendorRatingGradeDto>>
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
