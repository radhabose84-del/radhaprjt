
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderRootCause
{
    public class GetWorkOrderRootCauseQueryHandler  : IRequestHandler<GetWorkOrderRootCauseQuery, ApiResponseDTO<List<GetMiscMasterDto>>>
    {
        private readonly IWorkOrderQueryRepository _repository;
        private readonly IMapper _mapper;

        public GetWorkOrderRootCauseQueryHandler(IWorkOrderQueryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<GetMiscMasterDto>>> Handle(GetWorkOrderRootCauseQuery request, CancellationToken cancellationToken)
        {
            var DepMethod = await _repository.GetWORootCauseDescAsync();
            var DepMethodDtoList = _mapper.Map<List<GetMiscMasterDto>>(DepMethod);

            return new ApiResponseDTO<List<GetMiscMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = DepMethodDtoList,
                TotalCount = DepMethodDtoList.Count
            };
        }
    }
}