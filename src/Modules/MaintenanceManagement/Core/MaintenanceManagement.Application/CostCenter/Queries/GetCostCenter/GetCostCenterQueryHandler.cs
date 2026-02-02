using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter
{
    public class GetCostCenterQueryHandler : IRequestHandler<GetCostCenterQuery, ApiResponseDTO<List<CostCenterDto>>>
    {
        private readonly ICostCenterQueryRepository _iCostCenterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;


        public GetCostCenterQueryHandler(ICostCenterQueryRepository iCostCenterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iCostCenterQueryRepository = iCostCenterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CostCenterDto>>> Handle(GetCostCenterQuery request, CancellationToken cancellationToken)
        {

            var (list, totalCount) =
                await _iCostCenterQueryRepository.GetAllCostCenterListGroupAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm);

            list ??= new List<CostCenterDto>();

            // 📘 Log domain event
            await _mediator.Publish(new AuditLogsDomainEvent(
             actionDetail: "GetCostCenter",
             actionCode: "Get",
             actionName: list.Count.ToString(),
             details: "CostCenter details were fetched.",
             module: "CostCenter"
         ), cancellationToken);

            // ✅ Return
            return new ApiResponseDTO<List<CostCenterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                // Data = costCenterDtos,
                Data = list,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }



    }
}