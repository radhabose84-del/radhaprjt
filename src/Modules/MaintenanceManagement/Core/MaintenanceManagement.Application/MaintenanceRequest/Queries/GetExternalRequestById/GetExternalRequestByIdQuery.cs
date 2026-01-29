using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExternalRequestById
{
     public class GetExternalRequestsByIdsQuery : IRequest<ApiResponseDTO<List<GetExternalRequestByIdDto>>>
    {
        public List<int>? Ids { get; set; }  // List of IDs to fetch multiple records
    }
}