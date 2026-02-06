using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.External.IUser;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExternalRequestById
{
    public class GetExternalRequestByIdQueryHandler : IRequestHandler<GetExternalRequestsByIdsQuery, ApiResponseDTO<List<GetExternalRequestByIdDto>>>
    {

        private readonly IMaintenanceRequestQueryRepository _maintenanceRequestQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
               public GetExternalRequestByIdQueryHandler(IMaintenanceRequestQueryRepository maintenanceRequestQueryRepository, IMapper mapper, IMediator mediator)
        {
            _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
           
        }
        public async Task<ApiResponseDTO<List<GetExternalRequestByIdDto>>> Handle(GetExternalRequestsByIdsQuery request, CancellationToken cancellationToken)
        {

            if (request.Ids == null || !request.Ids.Any())
            {
                return new ApiResponseDTO<List<GetExternalRequestByIdDto>>
                {
                    IsSuccess = false,
                    Message = "No IDs provided.",
                    Data = new List<GetExternalRequestByIdDto>()
                };
            }

            var externalRequests = await _maintenanceRequestQueryRepository.GetExternalRequestByIdAsync(request.Ids);

           
            if (!externalRequests.Any())
            {
                return new ApiResponseDTO<List<GetExternalRequestByIdDto>>
                {
                    IsSuccess = false,
                    Message = "No external requests found for the provided IDs.",
                    Data = new List<GetExternalRequestByIdDto>()
                };
            }

            return new ApiResponseDTO<List<GetExternalRequestByIdDto>>
            {
                IsSuccess = true,
                Message = "External requests fetched successfully.",
                Data = externalRequests
            };
        }


    }
}