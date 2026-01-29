
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderSource
{
    public class GetWorkOrderSourceQueryHandler  : IRequestHandler<GetWorkOrderSourceQuery, ApiResponseDTO<List<GetMiscMasterDto>>>
    {
        private readonly IWorkOrderQueryRepository _repository;
        private readonly IMapper _mapper;

        public GetWorkOrderSourceQueryHandler(IWorkOrderQueryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<GetMiscMasterDto>>> Handle(GetWorkOrderSourceQuery request, CancellationToken cancellationToken)
        {
            var DepMethod = await _repository.GetWOSourceDescAsync();
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