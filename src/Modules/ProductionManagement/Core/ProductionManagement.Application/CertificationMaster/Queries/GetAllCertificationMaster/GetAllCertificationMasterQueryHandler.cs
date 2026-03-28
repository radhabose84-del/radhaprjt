using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CertificationMaster.Queries.GetAllCertificationMaster
{
    public class GetAllCertificationMasterQueryHandler : IRequestHandler<GetAllCertificationMasterQuery, ApiResponseDTO<List<CertificationMasterDto>>>
    {
        private readonly ICertificationMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllCertificationMasterQueryHandler(
            ICertificationMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CertificationMasterDto>>> Handle(GetAllCertificationMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<CertificationMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllCertificationMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Certification Master details were fetched.",
                module: "CertificationMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<CertificationMasterDto>>
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
