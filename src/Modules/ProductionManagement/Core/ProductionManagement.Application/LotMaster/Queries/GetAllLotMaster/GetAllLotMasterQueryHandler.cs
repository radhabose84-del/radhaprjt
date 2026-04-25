using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Application.LotMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.LotMaster.Queries.GetAllLotMaster
{
    public class GetAllLotMasterQueryHandler : IRequestHandler<GetAllLotMasterQuery, ApiResponseDTO<List<LotMasterDto>>>
    {
        private readonly ILotMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllLotMasterQueryHandler(
            ILotMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<LotMasterDto>>> Handle(
            GetAllLotMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, request.ItemId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllLotMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "LotMaster details were fetched.",
                module: "LotMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<LotMasterDto>>
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
