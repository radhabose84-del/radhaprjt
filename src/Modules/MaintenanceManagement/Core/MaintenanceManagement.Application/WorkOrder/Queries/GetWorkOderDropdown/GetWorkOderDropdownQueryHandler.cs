
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOderDropdown
{
    public class GetWorkOderDropdownQueryHandler : IRequestHandler<GetWorkOderDropdownQuery, ApiResponseDTO<List<GetWorkOderDropdownDto>>>
    {
        private readonly IWorkOrderQueryRepository _repository;
        private readonly IMapper _mapper;

        public GetWorkOderDropdownQueryHandler(IWorkOrderQueryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<GetWorkOderDropdownDto>>> Handle(GetWorkOderDropdownQuery request, CancellationToken cancellationToken)
        {
            var DepMethod = await _repository.GetWorkOrderAsync();
            var DepMethodDtoList = _mapper.Map<List<GetWorkOderDropdownDto>>(DepMethod);

            return new ApiResponseDTO<List<GetWorkOderDropdownDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = DepMethodDtoList,
                TotalCount = DepMethodDtoList.Count
            };
        }
    }
}