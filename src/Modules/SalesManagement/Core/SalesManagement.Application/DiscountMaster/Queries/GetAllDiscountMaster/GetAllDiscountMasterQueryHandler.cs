using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DiscountMaster.Queries.GetAllDiscountMaster
{
    public class GetAllDiscountMasterQueryHandler : IRequestHandler<GetAllDiscountMasterQuery, ApiResponseDTO<List<DiscountMasterDto>>>
    {
        private readonly IDiscountMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllDiscountMasterQueryHandler(IDiscountMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<DiscountMasterDto>>> Handle(GetAllDiscountMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllDiscountMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "DiscountMaster details were fetched.",
                module: "DiscountMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<DiscountMasterDto>>
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
