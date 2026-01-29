using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster
{
    public class GetSpecificationMasterQueryHandler : IRequestHandler<GetSpecificationMasterQuery, ApiResponseDTO<List<SpecificationMasterDTO>>>
    {
        private readonly ISpecificationMasterQueryRepository _specificationMasterRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetSpecificationMasterQueryHandler(ISpecificationMasterQueryRepository specificationMasterRepository , IMapper mapper, IMediator mediator)
        {
            _specificationMasterRepository = specificationMasterRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<List<SpecificationMasterDTO>>> Handle(GetSpecificationMasterQuery request, CancellationToken cancellationToken)
        {
            var (specificationMaster, totalCount) = await _specificationMasterRepository.GetAllSpecificationGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var specificationMasterList = _mapper.Map<List<SpecificationMasterDTO>>(specificationMaster);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"SpecificationMaster details was fetched.",
                module:"SpecificationMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<SpecificationMasterDTO>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = specificationMasterList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };            
        }
    }
}