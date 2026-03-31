using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster
{
    public class GetAllTncTemplateQueryHandler : IRequestHandler<GetAllTncTemplateQuery, ApiResponseDTO<List<TncTemplateMasterDto>>>
    {
        private readonly ITnCTemplateMasterQueryRepository _tnCTemplateMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllTncTemplateQueryHandler(ITnCTemplateMasterQueryRepository tnCTemplateMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _tnCTemplateMasterQueryRepository = tnCTemplateMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<TncTemplateMasterDto>>> Handle(GetAllTncTemplateQuery request, CancellationToken cancellationToken)
        {
              var (items, totalCount) = await _tnCTemplateMasterQueryRepository.GetAllTncTemplateAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            
            return new ApiResponseDTO<List<TncTemplateMasterDto>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = items.Count > 0 ? "TnC templates fetched successfully." : "No TnC templates found.",
                Data = items,

                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}