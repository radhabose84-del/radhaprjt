#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.BusinessUnit.Dto;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.BusinessUnit.Queries.GetAllBusinessUnit
{
    public class GetAllBusinessUnitQueryHandler : IRequestHandler<GetAllBusinessUnitQuery, ApiResponseDTO<List<BusinessUnitDto>>>
    {
        private readonly IBusinessUnitQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllBusinessUnitQueryHandler(IBusinessUnitQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<BusinessUnitDto>>> Handle(GetAllBusinessUnitQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm);

            var businessUnitDtos = _mapper.Map<List<BusinessUnitDto>>(data);

            // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllBusinessUnitQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "BusinessUnit details were fetched.",
                module: "BusinessUnit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<BusinessUnitDto>>
            {
                IsSuccess = true,
                Message = "Business Units retrieved successfully",
                Data = businessUnitDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
